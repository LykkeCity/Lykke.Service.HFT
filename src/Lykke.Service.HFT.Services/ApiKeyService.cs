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
        private readonly CacheSettings _settings;

        public ApiKeyService(IDistributedCache distributedCache, CacheSettings settings)
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
            var clientId = await _distributedCache.GetStringAsync(_settings.GetApiKey(apiKey));
            return clientId;
        }

        public async Task<string> GetNotificationIdAsync(string walletId)
        {
            var apiKey = await _distributedCache.GetStringAsync(_settings.GetNotificationId(walletId));
            return apiKey;
        }

        public async Task SetNotificationIdAsync(string apiKey, string notificationId)
        {
            var walletId = await GetClientAsync(apiKey);
            await _distributedCache.SetStringAsync(_settings.GetNotificationId(walletId), notificationId);
        }
    }
}
