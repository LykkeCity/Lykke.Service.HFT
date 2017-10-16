using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Services.Messages;

namespace Lykke.Service.HFT.Services
{
	public class LimitOrdersConsumer : IDisposable
	{
		private readonly ILog _log;
		private readonly IRepository<LimitOrderState> _orderStateRepository;
		private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
		private const string QueueName = "highfrequencytrading";
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

		private async Task ProcessLimitOrder(LimitOrderMessage limitOrder)
		{
			foreach (var order in limitOrder.Orders)
			{
				if (Guid.TryParse(order.Order.ExternalId, out Guid orderId))
				{
					// todo: use 'update' request only for better performance
					var orderState = await _orderStateRepository.Get(orderId);
					if (orderState != null)
					{
						// todo: use automapper
						orderState.Status = order.Order.Status;
						//orderState.ClientId = order.Order.ClientId;
						//orderState.AssetPairId = order.Order.AssetPairId;
						orderState.Volume = order.Order.Volume;
						//orderState.Price = order.Order.Price;
						orderState.RemainingVolume = order.Order.RemainingVolume;
						orderState.LastMatchTime = order.Order.LastMatchTime;
						orderState.CreatedAt = order.Order.CreatedAt;
						orderState.Registered = order.Order.Registered;
						await _orderStateRepository.Update(orderState);
					}
				}
			}
		}

		public void Dispose()
		{
			_subscriber.Stop();
		}
	}
}
