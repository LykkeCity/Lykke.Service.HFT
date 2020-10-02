using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Services.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services.Projections
{
    public class ApiKeyProjection
    {
        private readonly ILog _log;
        private readonly IDistributedCache _distributedCache;

        public ApiKeyProjection(
            [NotNull] ILogFactory logFactory,
            [NotNull] IDistributedCache distributedCache)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task Handle(ApiKeyUpdatedEvent evt)
        {
            _log.Info($"enabled: {evt.Enabled}", context: evt.ApiKey.Substring(0, 4));
            var tasks = new List<Task>();

            if (evt.Enabled && !evt.Apiv2Only)
            {
                tasks.Add(_distributedCache.SetStringAsync(Constants.GetKeyForApiKey(evt.ApiKey), evt.WalletId));
                tasks.Add(_distributedCache.SetAsync(Constants.GetKeyForWalletId(evt.WalletId), new byte[] { 1 }));
            }
            else
            {
                tasks.Add(_distributedCache.RemoveAsync(Constants.GetKeyForApiKey(evt.ApiKey)));
                tasks.Add(_distributedCache.RemoveAsync(Constants.GetKeyForWalletId(evt.WalletId)));
            }

            await Task.WhenAll(tasks);
        }

    }
}
