using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.RabbitMqBroker;
using System.Threading.Tasks;
using Lykke.Service.HFT.Services.Events;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using System.Linq;
using Lykke.Service.HFT.Core.Services;
using Common.Log;

namespace Lykke.Service.HFT.Services.Consumers
{
    public class ClientSettingsUpdatesConsumer : IStartable, IStopable
    {
        private readonly RabbitMqSubscriber<ClientSettingsCashoutBlockUpdated> _subscriber;
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IApiKeysCacheService _apiKeysCache;
        private readonly ILog _log;

        public ClientSettingsUpdatesConsumer(ILogFactory logFactory,
            string connectionString,
            IRepository<ApiKey> apiKeyRepository,
            IApiKeysCacheService apiKeysCache)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeysCache = apiKeysCache;
            _log = logFactory.CreateLog(this);

            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = connectionString,
                QueueName = $"client-account.client-settings-updated.hft-api",
                ExchangeName = $"client-account.client-settings-updated",
                RoutingKey = nameof(ClientSettingsCashoutBlockUpdated),
                IsDurable = false
            };

            var strategy = new DefaultErrorHandlingStrategy(logFactory, subscriptionSettings);

            _subscriber = new RabbitMqSubscriber<ClientSettingsCashoutBlockUpdated>(logFactory, subscriptionSettings, strategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<ClientSettingsCashoutBlockUpdated>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .Subscribe(HandleMessage);           
        }

        public async Task HandleMessage(ClientSettingsCashoutBlockUpdated evt)
        {
            // Race condition with ApiKeyUpdatedEvent event handling is possible, but it is decided
            // that it's acceptable since these events are not very frequent

            _log.Info($"Got client trades blocking update. Trades are blocked: {evt.TradesBlocked}", context: new
            {
                ClientId = evt.ClientId
            });

            var enabledClientApiKeys = (_apiKeyRepository.FilterBy(x => x.ClientId == evt.ClientId && x.ValidTill == null && !x.Apiv2Only)).ToList();

            if (evt.TradesBlocked)
            {
                var tasks = enabledClientApiKeys.Select(x => _apiKeysCache.RemoveKey(x.Token ?? x.Id.ToString()));

                await Task.WhenAll(tasks);

                foreach(var key in enabledClientApiKeys)
                {
                    var apiKeyStart = (key.Token ?? key.Id.ToString()).Substring(0, 4);

                    _log.Info($"API key has been cached", context: new
                    {
                        ClientId = evt.ClientId,
                        ApiKeyStart = apiKeyStart
                    });
                }
            }
            else
            {
                foreach (var key in enabledClientApiKeys)
                {
                    var apiKeyStart = (key.Token ?? key.Id.ToString()).Substring(0, 4);

                    _log.Info($"API key has been evicted from the cache", context: new
                    {
                        ClientId = evt.ClientId,
                        ApiKeyStart = apiKeyStart
                    });
                }

                await _apiKeysCache.AddApiKeys(enabledClientApiKeys);
            }
        }

        public void Start()
        {
            _subscriber.Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }
    }
}
