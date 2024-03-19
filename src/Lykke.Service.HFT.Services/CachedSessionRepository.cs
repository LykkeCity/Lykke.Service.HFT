using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.HFT.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.HFT.Services
{
    public class CachedSessionRepository : ISessionRepository
    {
        private readonly IMemoryCache _cache;
        private readonly IApiKeysCacheService _apiKeysCacheService;
        private readonly MemoryCacheEntryOptions _sessionCacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(14));

        public CachedSessionRepository(IMemoryCache cache, IApiKeysCacheService hftClientService)
        {
            _cache = cache;
            _apiKeysCacheService = hftClientService;
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
            var walletId = _apiKeysCacheService.GetWalletIdAsync(token).GetAwaiter().GetResult();
            _cache.Set(sessionId, walletId, _sessionCacheOptions);

            if (_cache.TryGetValue(walletId, out long[] sessionIds))
            {
                // working with HashSet is not effective, but more readable than working with Array; type 'long[]' is stored for performance needs
                var sessions = new HashSet<long>(sessionIds) { sessionId };
                _cache.Set(walletId, sessions.ToArray(), _sessionCacheOptions);
            }
            else
            {
                _cache.Set(walletId, new[] { sessionId }, _sessionCacheOptions);
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
