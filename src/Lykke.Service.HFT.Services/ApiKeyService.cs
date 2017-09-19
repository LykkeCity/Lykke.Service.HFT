using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyService : IApiKeyValidator, IClientResolver
    {
        private readonly IDistributedCache _distributedCache;
        private readonly AppSettings.HighFrequencyTradingSettings _settings;

        public ApiKeyService(IDistributedCache distributedCache, AppSettings.HighFrequencyTradingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<bool> ValidateAsync(string apiKey)
        {
            var clientId = await GetClientAsync(apiKey);
            return clientId != null;
        }

        public async Task<string> GetClientAsync(string apiKey)
        {
            var clientId = await _distributedCache.GetStringAsync(GetCacheKey(apiKey));
            if (clientId == null && apiKey == _settings.ApiKey)
            {
                return _settings.ApiKey;
            }
            return clientId;
        }
        private string GetCacheKey(string apiKey)
        {
            return _settings.CacheSettings.GetApiKey(apiKey);
        }
    }
}
