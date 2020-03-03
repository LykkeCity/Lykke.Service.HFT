using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Sdk;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WampSharp.V2.Realm;

namespace Lykke.Service.HFT.Services
{
    [UsedImplicitly]
    internal class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IEnumerable<IWampHostedRealm> _realms;
        private readonly ISessionRepository _sessionRepository;
        private readonly IApiKeyCacheInitializer _apiKeyCacheInitializer;
        [NotNull] private readonly ICqrsEngine _cqrsEngine;

        public StartupManager(
            ILogFactory logFactory,
            IEnumerable<IWampHostedRealm> realms,
            ISessionRepository sessionRepository,
            IApiKeyCacheInitializer apiKeyCacheInitializer,
            [NotNull] ICqrsEngine cqrsEngine)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
            _realms = realms;
            _sessionRepository = sessionRepository;
            _apiKeyCacheInitializer = apiKeyCacheInitializer;
            _cqrsEngine = cqrsEngine;
        }

        public async Task StartAsync()
        {
            _cqrsEngine.StartSubscribers();
            _cqrsEngine.StartProcesses();

            _log.Info("Subscribing to the realm sessions...");

            foreach (var realm in _realms)
            {
                realm.SessionClosed += (sender, args) => { _sessionRepository.TryRemoveSessionId(args.SessionId); };
            }

#if !DEBUG
            await _apiKeyCacheInitializer.InitApiKeyCache();
#endif
        }
    }
}
