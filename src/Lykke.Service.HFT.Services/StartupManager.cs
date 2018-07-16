using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
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
        private readonly ISessionRepository _sessionRepository;
        private readonly IApiKeyCacheInitializer _apiKeyCacheInitializer;

        public StartupManager(
            ILogFactory logFactory,
            IEnumerable<IWampHostedRealm> realms,
            ISessionRepository sessionRepository,
            IApiKeyCacheInitializer apiKeyCacheInitializer,
            [NotNull] ICqrsEngine cqrs)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
            _realms = realms;
            _sessionRepository = sessionRepository;
            _apiKeyCacheInitializer = apiKeyCacheInitializer;

            if (cqrs == null) throw new ArgumentNullException(nameof(cqrs)); // is needed for bootstrap
        }

        public async Task StartAsync()
        {
            _log.Info("Subscribing to the realm sessions...");

            foreach (var realm in _realms)
            {
                realm.SessionClosed += (sender, args) => { _sessionRepository.TryRemoveSessionId(args.SessionId); };
            }

            await _apiKeyCacheInitializer.InitApiKeyCache();
        }
    }
}
