using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IEnumerable<IWampHostedRealm> _realms;
        private readonly ISessionCache _sessionCache;
        private readonly IApiKeyCacheInitializer _apiKeyCacheInitializer;

        public StartupManager(
            ILog log,
            IEnumerable<IWampHostedRealm> realms,
            ISessionCache sessionCache,
            IApiKeyCacheInitializer apiKeyCacheInitializer)
        {
            _log = log;
            _realms = realms;
            _sessionCache = sessionCache;
            _apiKeyCacheInitializer = apiKeyCacheInitializer;
        }

        public async Task StartAsync()
        {
            _log.WriteInfo(nameof(StartAsync), "", "Subscribing to the realm sessions...");

            foreach (var realm in _realms)
            {
                realm.SessionClosed += (sender, args) => { _sessionCache.TryRemoveSessionId(args.SessionId); };
            }

            await _apiKeyCacheInitializer.InitApiKeyCache();
        }
    }
}
