using System;
using System.Collections.Async;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Services.Consumers.Messages;

namespace Lykke.Service.HFT.Services.Consumers
{
    public class LimitOrdersConsumer : IDisposable
    {
        private readonly ILog _log;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
        private const string QueueName = "highfrequencytrading-api";
        private const bool QueueDurable = false;

        public LimitOrdersConsumer(ILog log, AppSettings.RabbitMqSettings settings, IRepository<LimitOrderState> orderStateRepository)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
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
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
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
                        // these properties cannot change: Id, ClientId, AssetPairId, Price; ignoring them
                        orderState.Status = order.Order.Status;
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

        public void Dispose()
        {
            _subscriber.Stop();
        }
    }
}
