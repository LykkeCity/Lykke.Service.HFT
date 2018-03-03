using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Services.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services.Projections
{
    public class ApiKeyProjection
    {
        private readonly ILog _log;
        private readonly IDistributedCache _distributedCache;
        private readonly CacheSettings _cacheSettings;

        public ApiKeyProjection(
            [NotNull] ILog log,
            [NotNull] IDistributedCache distributedCache,
            [NotNull] CacheSettings cacheSettings)
        {
            _log = log.CreateComponentScope(nameof(ApiKeyProjection));
            _cacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task Handle(ApiKeyUpdatedEvent evt)
        {
            _log.WriteInfo(nameof(ApiKeyUpdatedEvent), evt.ApiKey.Substring(0, 4), $"enabled: {evt.Enabled}");
            if (evt.Enabled)
            {
                await _distributedCache.SetStringAsync(_cacheSettings.GetKeyForApiKey(evt.ApiKey), evt.WalletId);
                await _distributedCache.SetAsync(_cacheSettings.GetKeyForWalletId(evt.WalletId), new byte[] { 1 });
            }
            else
            {
                await _distributedCache.RemoveAsync(_cacheSettings.GetKeyForApiKey(evt.ApiKey));
                await _distributedCache.RemoveAsync(_cacheSettings.GetKeyForWalletId(evt.WalletId));
            }
        }

    }
}
