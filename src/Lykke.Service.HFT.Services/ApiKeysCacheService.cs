using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeysCacheService : IApiKeysCacheService
    {
        // This class contains all sorts of Redis related
        // interfaces because different methods were
        // copied from different places accross the repo
        // and it doesn't worth it get rid of IDistributedCache
        // and move to IDatabase 

        private readonly IServer _redisServer;
        private readonly IDatabase _redisDatabase;
        private readonly IDistributedCache _distributedCache;
        private readonly string _instanceName;

        public ApiKeysCacheService(IServer redisServer, IDatabase redisDatabase, IDistributedCache distributedCache, string instanceName)
        {
            _redisServer = redisServer;
            _redisDatabase = redisDatabase;
            _distributedCache = distributedCache;
            _instanceName = instanceName;
        }

        public async Task<string> GetWalletIdAsync(string apiKey)
        {
            var walletId = await _distributedCache.GetStringAsync(Constants.GetKeyForApiKey(apiKey));
            
            return walletId;
        }
        public async Task AddKey(string apiKey, string walletId)
        {
            await _distributedCache.SetStringAsync(Constants.GetKeyForApiKey(apiKey), walletId);
        }

        public async Task RemoveKey(string apiKey)
        {
            await _distributedCache.RemoveAsync(Constants.GetKeyForApiKey(apiKey));
        }

        public async Task AddApiKeys(IReadOnlyList<ApiKey> keys)
        {
            var tasks = new List<Task>();
            var batch = _redisDatabase.CreateBatch();

            foreach (var key in keys)
            {
                var cacheKey = Constants.GetKeyForApiKey(key.Token ?? key.Id.ToString());
                tasks.Add(batch.HashSetAsync(string.Concat(_instanceName, cacheKey), "data", key.WalletId));
            }

            batch.Execute();

            await Task.WhenAll(tasks);
        }

        public async Task Clear()
        {
            var keys = _redisServer
                .Keys(pattern: _instanceName + "*", pageSize: 1000)
                .ToArray();

            await _redisDatabase.KeyDeleteAsync(keys);
        }
    }
}
