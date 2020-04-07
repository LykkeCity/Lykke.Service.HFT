using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
    public class HftClientService : IHftClientService
    {
        private readonly IRepository<ApiKey> _repository;
        private readonly IDistributedCache _distributedCache;

        public HftClientService(
            IRepository<ApiKey> repository,
            IDistributedCache distributedCache)
        {
            _repository = repository;
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<string> GetWalletIdAsync(string apiKey)
        {
            var clientId = await _distributedCache.GetStringAsync(Constants.GetKeyForApiKey(apiKey));
            return clientId;
        }

        public Task<ApiKey> GetApiKey(string apiKey)
        {
            return Guid.TryParse(apiKey, out var id)
                ? _repository.Get(id)
                : Task.FromResult<ApiKey>(null);
        }

        public async Task<bool> IsHftWalletAsync(string walletId)
        {
            var wallet = await _distributedCache.GetAsync(Constants.GetKeyForWalletId(walletId));
            return wallet != null && wallet[0] == 1;
        }
    }
}
