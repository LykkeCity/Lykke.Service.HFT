using System;
using System.Collections.Async;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Contracts.Events;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Wamp.Consumers.Messages;
using WampSharp.V2;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Wamp.Consumers
{
    public class LimitOrdersConsumer : IDisposable
    {
        private readonly IClientResolver _clientResolver;
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
        private const string QueueName = "highfrequencytrading-wamp";
        private const bool QueueDurable = false;
        private readonly IWampHostedRealm _realm;
        private const string TopicUri = "orders.limit";

        public LimitOrdersConsumer(ILog log,
            AppSettings.RabbitMqSettings settings,
            IRepository<LimitOrderState> orderStateRepository,
            IWampHostedRealm realm,
            [NotNull] IClientResolver clientResolver)
        {
            _clientResolver = clientResolver ?? throw new ArgumentNullException(nameof(clientResolver));

            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _realm = realm ?? throw new ArgumentNullException(nameof(realm));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));

            try
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = $"{settings.ExchangeName}.{QueueName}",
                    ExchangeName = settings.ExchangeName,
                    IsDurable = QueueDurable
                };
                _subscriber = new RabbitMqSubscriber<LimitOrderMessage>(subscriptionSettings, new DefaultErrorHandlingStrategy(_log, subscriptionSettings))
                    .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrderMessage>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(ProcessLimitOrder)
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(LimitOrdersConsumer), null, null, ex).Wait();
                throw;
            }
        }

        private async Task ProcessLimitOrder(LimitOrderMessage ordersUpdateMessage)
        {
            await ordersUpdateMessage.Orders.ParallelForEachAsync(async order =>
            {
                if (Guid.TryParse(order.Order.ExternalId, out Guid orderId))
                {
                    var orderState = await _orderStateRepository.Get(orderId);
                    // we are processing orders made by this service only
                    if (orderState != null)
                    {
                        var notifyResponse = new LimitOrderUpdateEvent
                        {
                            // todo: use AutoMapper
                            Order = new Order
                            {
                                Id = orderId,
                                Status = order.Order.Status == LimitOrderMessage.OrderStatus.ReservedVolumeGreaterThanBalance
                                    ? Contracts.Events.OrderStatus.NotEnoughFunds
                                    : (Contracts.Events.OrderStatus)order.Order.Status,
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

                        var notificationId = await _clientResolver.GetNotificationIdAsync(order.Order.ClientId);
                        if (notificationId == null)
                            return;

                        PublishEvent(notificationId, notifyResponse);
                    }
                }
            }).ConfigureAwait(false);
        }

        private void PublishEvent(string notificationId, LimitOrderUpdateEvent notifyResponse)
        {
            _realm.Services.GetSubject(TopicUri)
                .OnNext(new WampEvent
                {
                    Options = new PublishOptions
                    {
                        Eligible = new[] { long.Parse(notificationId) }
                    },
                    Arguments = new object[] { notifyResponse }
                });
        }

        public void Dispose()
        {
            _subscriber.Stop();
        }
    }
}
