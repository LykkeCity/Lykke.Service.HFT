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
        private readonly AppSettings.HighFrequencyTradingSettings _settings;
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IServer _redisServer;
        private readonly IDatabase _redisDatabase;

        public ApiKeyCacheInitializer(AppSettings.HighFrequencyTradingSettings settings, IRepository<ApiKey> orderStateRepository, IServer redisServer, IDatabase redisDatabase, IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _apiKeyRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _redisServer = redisServer ?? throw new ArgumentNullException(nameof(redisServer));
            _redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
        }

        public async Task InitApiKeyCache()
        {
            ClearExistingRecords();

            var keys = _apiKeyRepository.FilterBy(x => x.ValidTill == null).ToList();
            foreach (var key in keys)
            {
                await _distributedCache.SetStringAsync(GetCacheKey(key.Id.ToString()), key.ClientId);
                //_redisDatabase.StringSet(_settings.CacheSettings.ApiKeyCacheInstance + GetCacheKey(key.Id.ToString()), key.ClientId);
            }
        }

        private string GetCacheKey(string apiKey)
        {
            return _settings.CacheSettings.GetApiKey(apiKey);
        }

        private void ClearExistingRecords()
        {
            var keys = _redisServer.Keys(pattern: _settings.CacheSettings.ApiKeyCacheInstance + "*").ToArray();
            _redisDatabase.KeyDelete(keys);
        }
    }
}
