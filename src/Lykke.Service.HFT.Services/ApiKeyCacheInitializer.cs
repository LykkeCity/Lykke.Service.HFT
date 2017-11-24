using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyCacheInitializer : IApiKeyCacheInitializer
    {
        private readonly IDistributedCache _distributedCache;
        private readonly CacheSettings _settings;
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IServer _redisServer;
        private readonly IDatabase _redisDatabase;

        public ApiKeyCacheInitializer(CacheSettings settings, IRepository<ApiKey> orderStateRepository, IServer redisServer, IDatabase redisDatabase, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _apiKeyRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _redisServer = redisServer ?? throw new ArgumentNullException(nameof(redisServer));
            _redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
        }

        public async Task InitApiKeyCache()
        {
            return;
            ClearExistingRecords();

            var keys = _apiKeyRepository.FilterBy(x => x.ValidTill == null).ToList();
            foreach (var key in keys)
            {
                await _distributedCache.SetStringAsync(GetCacheKey(key.Id.ToString()), key.WalletId);
            }
        }

        private string GetCacheKey(string apiKey)
        {
            return _settings.GetApiKey(apiKey);
        }

        private void ClearExistingRecords()
        {
            var keys = _redisServer.Keys(pattern: _settings.ApiKeyCacheInstance + "*").ToArray();
            _redisDatabase.KeyDelete(keys);
        }
    }
}
