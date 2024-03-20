using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyCacheInitializer : IApiKeyCacheInitializer
    {
        private readonly ILog _log;
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IApiKeysCacheService _apiKeysCache;
        private readonly IBlockedClientsService _blockedClients;

        public ApiKeyCacheInitializer(
            ILogFactory logFactory,
            IRepository<ApiKey> orderStateRepository, 
            IApiKeysCacheService apiKeysCache, 
            IBlockedClientsService blockedClients)
        {
            _log = logFactory.CreateLog(this);
            _apiKeyRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _apiKeysCache = apiKeysCache;
            _blockedClients = blockedClients;
        }

        public async Task InitApiKeyCache()
        {
            _log.Info("API keys cache is being initialized");

            var keys = await GetApiKeys();
            await _apiKeysCache.Clear();
            // Clients may get 401 between these two calls.
            // It would be good to rework this.
            await _apiKeysCache.AddApiKeys(keys);

            _log.Info($"API keys cache has been initialized. {keys.Count} active keys were added to the cache");
        }

        private async Task<List<ApiKey>> GetApiKeys()
        {
            var validV1ApiKeys = _apiKeyRepository.FilterBy(x => x.ValidTill == null && !x.Apiv2Only);
            var enabledApiKeys = new List<ApiKey>();

            foreach(var key in validV1ApiKeys)
            {
                if(!await _blockedClients.IsClientBlocked(key.ClientId))
                {
                    enabledApiKeys.Add(key);
                }
            }

            return enabledApiKeys;
        }
    }
}
