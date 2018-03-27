using System;
using System.Collections.Async;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services.Consumers.Messages;

namespace Lykke.Service.HFT.Services.Consumers
{
    public class LimitOrdersConsumer : IDisposable
    {
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
        private readonly IHftClientService _hftClientService;
        private const string QueueName = "highfrequencytrading-api";
        private const bool QueueDurable = false;

        public LimitOrdersConsumer(
            ILog log,
            AppSettings.RabbitMqSettings settings,
            IRepository<LimitOrderState> orderStateRepository,
            [NotNull] IHftClientService hftClientService)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _hftClientService = hftClientService ?? throw new ArgumentNullException(nameof(hftClientService));

            try
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = $"{settings.ExchangeName}.{QueueName}",
                    ExchangeName = settings.ExchangeName,
                    IsDurable = QueueDurable
                };
                _subscriber = new RabbitMqSubscriber<LimitOrderMessage>(subscriptionSettings, new ResilientErrorHandlingStrategy(_log, subscriptionSettings,
                        retryTimeout: TimeSpan.FromSeconds(20),
                        retryNum: 3,
                        next: new DefaultErrorHandlingStrategy(_log, subscriptionSettings)))

                    .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrderMessage>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(ProcessLimitOrder)
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        private async Task ProcessLimitOrder(LimitOrderMessage ordersUpdateMessage)
        {
            await ordersUpdateMessage.Orders.ParallelForEachAsync(async order =>
            {
                if (Guid.TryParse(order.Order.ExternalId, out Guid orderId) && await _hftClientService.IsHftWalletAsync(order.Order.ClientId))
                {
                    var orderState = await _orderStateRepository.Get(orderId);
                    // we are processing orders made by this service only
                    if (orderState != null)
                    {
                        if (IsFinalStatus(orderState.Status))
                        {
                            _log.WriteWarning(nameof(ProcessLimitOrder), order, "Got update for order in final state. Ignoring.");
                            return;
                        }

                        // these properties cannot change: Id, ClientId, AssetPairId, Price; ignoring them
                        orderState.Status = Enum.TryParse(order.Order.Status.ToString(), out OrderStatus status) ? status : OrderStatus.Runtime;
                        orderState.Volume = order.Order.Volume;
                        orderState.RemainingVolume = order.Order.RemainingVolume;
                        orderState.LastMatchTime = order.Order.LastMatchTime;
                        orderState.CreatedAt = order.Order.CreatedAt;
                        orderState.Registered = order.Order.Registered;
                        await _orderStateRepository.Update(orderState);
                    }
                }
            }).ConfigureAwait(false);
        }

        private bool IsFinalStatus(OrderStatus status)
        {
            return status != OrderStatus.Pending && status != OrderStatus.InOrderBook && status != OrderStatus.Processing;
        }

        public void Dispose()
        {
            _subscriber?.Stop();
        }
    }
}
