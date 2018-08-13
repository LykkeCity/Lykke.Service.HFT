using System;
using System.Collections.Async;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Contracts.Events;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Wamp.Consumers.Messages;
using WampSharp.V2;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Wamp.Consumers
{
    public class LimitOrdersConsumer : IDisposable
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
        private const string QueueName = "highfrequencytrading-wamp";
        private const bool QueueDurable = false;
        private const string TopicUri = "orders.limit";
        private readonly IWampSubject _subject;

        public LimitOrdersConsumer(ILogFactory logFactory,
            RabbitMqSettings settings,
            IRepository<LimitOrderState> orderStateRepository,
            IWampHostedRealm realm,
            ISessionRepository sessionRepository)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }

            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _subject = realm.Services.GetSubject(TopicUri);

            try
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = $"{settings.ExchangeName}.{QueueName}",
                    ExchangeName = settings.ExchangeName,
                    IsDurable = QueueDurable
                };
                var strategy = new DefaultErrorHandlingStrategy(logFactory, subscriptionSettings);
                _subscriber = new RabbitMqSubscriber<LimitOrderMessage>(logFactory, subscriptionSettings, strategy)
                    .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrderMessage>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(ProcessLimitOrder)
                    .Start();
            }
            catch (Exception ex)
            {
                var log = logFactory.CreateLog(this);
                log.Error(ex);
                throw;
            }
        }

        private async Task ProcessLimitOrder(LimitOrderMessage ordersUpdateMessage)
        {
            await ordersUpdateMessage.Orders.ParallelForEachAsync(async order =>
            {
                if (Guid.TryParse(order.Order.ExternalId, out Guid orderId))
                {
                    var sessionIds = _sessionRepository.GetSessionIds(order.Order.ClientId);
                    if (sessionIds.Length == 0)
                        return;

                    var orderState = await _orderStateRepository.Get(orderId);
                    // we are processing orders made by this service only
                    if (orderState != null)
                    {
                        var notifyResponse = new LimitOrderUpdateEvent
                        {
                            Order = new Order
                            {
                                Id = orderId,
                                Status = Enum.TryParse(order.Order.Status, out OrderStatus status) ? status : OrderStatus.Runtime,
                                AssetPairId = order.Order.AssetPairId,
                                Volume = order.Order.Volume,
                                Price = order.Order.Price,
                                RemainingVolume = order.Order.RemainingVolume,
                                LastMatchTime = order.Order.LastMatchTime
                            },
                            Trades = order.Trades.Select(x => new Trade
                            {
                                Asset = x.Asset,
                                Volume = x.Volume,
                                Price = x.Price,
                                Timestamp = x.Timestamp,
                                OppositeAsset = x.OppositeAsset,
                                OppositeVolume = x.OppositeVolume
                            }).ToArray()
                        };

                        _subject.OnNext(new WampEvent
                        {
                            Options = new PublishOptions
                            {
                                Eligible = sessionIds
                            },
                            Arguments = new object[] { notifyResponse }
                        });
                    }
                }
            }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _subscriber?.Stop();
        }
    }
}
