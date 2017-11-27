using System;
using System.Collections.Async;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Wamp.Events;
using Lykke.Service.HFT.Wamp.Messages;
using WampSharp.V2.Realm;
using LimitOrderState = Lykke.Service.HFT.Core.Domain.LimitOrderState;
using Konscious.Security.Cryptography;

namespace Lykke.Service.HFT.Wamp
{
    public class LimitOrdersConsumer : IDisposable
    {
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
        private const string QueueName = "highfrequencytrading-wamp";
        private const bool QueueDurable = false;
        private readonly IWampHostedRealm _realm;
        private readonly HMACBlake2B _hashAlgorithm;

        public LimitOrdersConsumer(ILog log, AppSettings.RabbitMqSettings settings, IRepository<LimitOrderState> orderStateRepository, IWampHostedRealm realm)
        {
            _hashAlgorithm = new HMACBlake2B(128);
            _hashAlgorithm.Initialize();

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
                        // todo: get api-key by clientId
                        var clientId = order.Order.ClientId;
                        var subscriptionId = _hashAlgorithm.ComputeHash(clientId.ToUtf8Bytes()).ToBase64();
                        var userTopic = _realm.Services.GetSubject<LimitOrderUpdateEvent>($"orders.limit.wallet.{subscriptionId}");

                        var notifyResponse = new LimitOrderUpdateEvent
                        {
                            Order = new Events.Order
                            {
                                Id = orderId,
                                Status = order.Order.Status,
                                AssetPairId = order.Order.AssetPairId,
                                Volume = order.Order.Volume,
                                Price = order.Order.Price,
                                RemainingVolume = order.Order.RemainingVolume,
                                LastMatchTime = order.Order.LastMatchTime
                            },
                            Trades = order.Trades.Select(x => new Events.Trade
                            {
                                Asset = x.Asset,
                                Volume = x.Volume,
                                Price = x.Price,
                                Timestamp = x.Timestamp,
                                OppositeAsset = x.OppositeAsset,
                                OppositeVolume = x.OppositeVolume
                            }).ToArray()
                        };

                        userTopic.OnNext(notifyResponse);

                    }
                }
            }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _subscriber.Stop();
        }
    }
}
