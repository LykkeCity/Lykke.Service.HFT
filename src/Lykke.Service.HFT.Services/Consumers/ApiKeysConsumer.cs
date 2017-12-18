using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Services.Consumers.Messages;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services.Consumers
{
    public class ApiKeysConsumer : IDisposable
    {
        private readonly ILog _log;
        private readonly IDistributedCache _distributedCache;
        private readonly CacheSettings _cacheSettings;
        private readonly RabbitMqSubscriber<ApiKeyUpdatedMessage> _subscriber;
        private const string QueueName = "highfrequencytrading-api";
        private const bool QueueDurable = false;

        public ApiKeysConsumer(ILog log, AppSettings.RabbitMqSettings settings, IDistributedCache distributedCache, CacheSettings cacheSettings)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _cacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

            try
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = $"{settings.ExchangeName}.{QueueName}",
                    ExchangeName = settings.ExchangeName,
                    IsDurable = QueueDurable
                };
                _subscriber = new RabbitMqSubscriber<ApiKeyUpdatedMessage>(subscriptionSettings, new DefaultErrorHandlingStrategy(_log, subscriptionSettings))
                    .SetMessageDeserializer(new JsonMessageDeserializer<ApiKeyUpdatedMessage>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(Process)
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        private async Task Process(ApiKeyUpdatedMessage message)
        {
            var apiKey = message.ApiKey;
            await _log.WriteInfoAsync(nameof(ApiKeysConsumer), message.ApiKey.Id.ToString().Substring(0, 4), $"enabled: {message.ApiKey.Enabled}");
            if (apiKey.Enabled)
            {
                await _distributedCache.SetStringAsync(_cacheSettings.GetKeyForApiKey(apiKey.Id.ToString()),
                    apiKey.WalletId);
            }
            else
            {
                await _distributedCache.RemoveAsync(_cacheSettings.GetKeyForApiKey(apiKey.Id.ToString()));
                await _distributedCache.RemoveAsync(_cacheSettings.GetKeyForNotificationId(apiKey.WalletId));
            }
        }

        public void Dispose()
        {
            _subscriber.Stop();
        }
    }
}
