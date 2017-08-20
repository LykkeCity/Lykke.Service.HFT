using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Services.Messages;

namespace Lykke.Service.HFT.Services
{
	public class LimitOrdersConsumer : IDisposable
	{
		private readonly ILog _log;
		private readonly AppSettings.RabbitMqSettings _settings;
		private readonly RabbitMqSubscriber<LimitOrderMessage> _subscriber;
		private const string QueueName = "highfrequencytrading";
		private const bool QueueDurable = false;

		public LimitOrdersConsumer(ILog log, AppSettings.RabbitMqSettings settings)
		{
			_log = log ?? throw new ArgumentNullException(nameof(log));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

			try
			{
				var subscriptionSettings = new RabbitMqSubscriptionSettings
				{
					ConnectionString = _settings.ConnectionString,
					QueueName = $"{_settings.ExchangeName}.{QueueName}",
					ExchangeName = _settings.ExchangeName,
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
				var orderId = order.Order.ExternalId;
				if (MatchingEngineAdapter.LimitOrders.ContainsKey(orderId))
				{
					MatchingEngineAdapter.LimitOrders.AddOrUpdate(orderId, order.Order, (key, value) => order.Order);
				}
			}

			//try
			//{
			//	await _candlesManager.ProcessQuoteAsync(limitOrder);
			//}
			//catch (Exception ex)
			//{
			//	await _log.WriteErrorAsync(Constants.ComponentName, null, null, ex);
			//}
		}

		public void Dispose()
		{
			_subscriber.Stop();
		}
	}
}
