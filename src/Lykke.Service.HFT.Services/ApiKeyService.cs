using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.HFT.Services
{
    public class ApiKeyService : IApiKeyValidator, IClientResolver, ISessionCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly CacheSettings _settings;

        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _sessionCacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14));

        public ApiKeyService(IDistributedCache distributedCache, CacheSettings settings, IMemoryCache cache)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cache = cache;
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<bool> ValidateAsync(string apiKey)
        {
            var clientId = await GetClientAsync(apiKey);
            return clientId != null;
        }

        public async Task<string> GetClientAsync(string apiKey)
        {
            var clientId = await _distributedCache.GetStringAsync(_settings.GetKeyForApiKey(apiKey));
            // todo: request to HFT Internal Service
            return clientId;
        }

        private static readonly long[] ZeroSessionsValue = new long[0];
        public long[] GetSessionIds(string clientId)
        {
            if (_cache.TryGetValue(clientId, out long[] sessionIds))
            {
                return sessionIds;
            }

            return ZeroSessionsValue;
        }

        public void AddSessionId(string token, long sessionId)
        {
            var clientId = GetClientAsync(token).GetAwaiter().GetResult();
            _cache.Set(sessionId, clientId, _sessionCacheOptions);

            if (_cache.TryGetValue(clientId, out long[] sessionIds))
            {
                // working with HashSet is not effective, but more readable than working with Array; type 'long[]' is stored for performance needs
                var sessions = new HashSet<long>(sessionIds) { sessionId };
                _cache.Set(clientId, sessions.ToArray(), _sessionCacheOptions);
            }
            else
            {
                _cache.Set(clientId, new[] { sessionId }, _sessionCacheOptions);
            }
        }

        public bool TryRemoveSessionId(long sessionId)
        {
            if (_cache.TryGetValue(sessionId, out string clientId))
            {
                _cache.Remove(sessionId);

                // working with HashSet is not effective, but more readable than working with Array; type 'long[]' is stored for performance needs
                var sessionIds = _cache.Get<long[]>(clientId);
                var sessions = new HashSet<long>(sessionIds);
                sessions.Remove(sessionId);
                _cache.Set(clientId, sessions.ToArray(), _sessionCacheOptions);

                return true;
            }

            return false;
        }
    }
}
