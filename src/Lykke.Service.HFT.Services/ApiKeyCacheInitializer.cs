using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services.ApiKey;
using StackExchange.Redis;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyCacheInitializer : IApiKeyCacheInitializer
    {
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IServer _redisServer;
        private readonly IDatabase _redisDatabase;

        public ApiKeyCacheInitializer(IRepository<ApiKey> orderStateRepository, IServer redisServer, IDatabase redisDatabase)
        {
            _apiKeyRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _redisServer = redisServer ?? throw new ArgumentNullException(nameof(redisServer));
            _redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
        }

        public async Task InitApiKeyCache()
        {
            var keys = GetApiKeys();
            await ClearExistingRecords();
            await InsertValues(keys);
        }

        private async Task ClearExistingRecords()
        {
            var keys = _redisServer.Keys(pattern: Constants.ApiKeyCacheInstance + "*", pageSize: 1000).ToArray();
            await _redisDatabase.KeyDeleteAsync(keys);
        }

        private List<ApiKey> GetApiKeys()
        {
            return _apiKeyRepository.FilterBy(x => x.ValidTill == null).ToList();
        }

        private async Task InsertValues(List<ApiKey> keys)
        {
            var tasks = new List<Task>();
            var batch = _redisDatabase.CreateBatch();
            foreach (var key in keys)
            {
                tasks.Add(batch.HashSetAsync(Constants.ApiKeyCacheInstance + Constants.GetKeyForApiKey(key.Id.ToString()), "data", key.WalletId));
                tasks.Add(batch.HashSetAsync(Constants.ApiKeyCacheInstance + Constants.GetKeyForWalletId(key.WalletId), "data", new byte[] { 1 }));
            }
            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
