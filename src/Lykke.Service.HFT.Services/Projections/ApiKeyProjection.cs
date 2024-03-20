using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services.Events;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services.Projections
{
    public class ApiKeyProjection
    {
        private readonly ILog _log;
        private readonly IApiKeysCacheService _apiKeysCache;
        private readonly IBlockedClientsService _blockedClients;

        public ApiKeyProjection(
            [NotNull] ILogFactory logFactory,
            IApiKeysCacheService apiKeysCache, 
            IBlockedClientsService blockedClients)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
            _apiKeysCache = apiKeysCache;
            _blockedClients = blockedClients;
        }

        public async Task Handle(ApiKeyUpdatedEvent evt)
        {
            // Race condition with ClientSettingsCashoutBlockUpdated event handling is possible, but it is decided
            // that it's acceptable since these events are not very frequent

            var isClientBlocked = await _blockedClients.IsClientBlocked(evt.ClientId);
            var apiKeyStart = evt.ApiKey.Substring(0, 4);

            _log.Info($"API key enabled: {evt.Enabled}. Client blocked: {isClientBlocked}. Is V2 only: {evt.Apiv2Only}", context: new
            {
                ClientId = evt.ClientId,
                WalletId = evt.WalletId,
                ApiKeyStart = apiKeyStart
            });

            var tasks = new List<Task>();

            if (evt.Enabled && !evt.Apiv2Only && !isClientBlocked)
            {
                tasks.Add(_apiKeysCache.AddKey(evt.ApiKey, evt.WalletId));

                _log.Info($"API key has been cached", context: new
                {
                    ClientId = evt.ClientId,
                    WalletId = evt.WalletId,
                    ApiKeyStart = apiKeyStart
                });
            }
            else
            {
                tasks.Add(_apiKeysCache.RemoveKey(evt.ApiKey));

                _log.Info($"API key has been evicted from the cache", context: new
                {
                    ClientId = evt.ClientId,
                    WalletId = evt.WalletId,
                    ApiKeyStart = apiKeyStart
                });
            }

            await Task.WhenAll(tasks);
        }

    }
}
