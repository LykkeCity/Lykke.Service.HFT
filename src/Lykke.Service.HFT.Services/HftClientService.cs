using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
    public class HftClientService : IHftClientService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly CacheSettings _settings;

        public HftClientService(IDistributedCache distributedCache, CacheSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<string> GetWalletIdAsync(string apiKey)
        {
            var clientId = await _distributedCache.GetStringAsync(_settings.GetKeyForApiKey(apiKey));
            return clientId;
        }

        public async Task<bool> IsHftWalletAsync(string walletId)
        {
            var wallet = await _distributedCache.GetAsync(_settings.GetKeyForWalletId(walletId));
            return wallet != null && wallet[0] == 1;
        }
    }
}
