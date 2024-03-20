using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Sdk;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Core.Services.ApiKey;
using Lykke.Service.HFT.Services.Consumers;
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
        [NotNull] private readonly ICqrsEngine _cqrs;
        private readonly ClientSettingsUpdatesConsumer _clientSettingsUpdatesConsumer;

        public StartupManager(
            ILogFactory logFactory,
            IEnumerable<IWampHostedRealm> realms,
            ISessionRepository sessionRepository,
            IApiKeyCacheInitializer apiKeyCacheInitializer,
            ICqrsEngine cqrs,
            ClientSettingsUpdatesConsumer clientSettingsUpdatesConsumer)
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
            _realms = realms;
            _sessionRepository = sessionRepository;
            _apiKeyCacheInitializer = apiKeyCacheInitializer;
            _cqrs = cqrs;
            _clientSettingsUpdatesConsumer = clientSettingsUpdatesConsumer;
        }

        public async Task StartAsync()
        {
#if !DEBUG
            await _apiKeyCacheInitializer.InitApiKeyCache();
#endif

            _cqrs.StartSubscribers();
            _cqrs.StartProcesses();

            _log.Info("Subscribing to the realm sessions...");

            foreach (var realm in _realms)
            {
                realm.SessionClosed += (sender, args) => { _sessionRepository.TryRemoveSessionId(args.SessionId); };
            }

            _clientSettingsUpdatesConsumer.Start();
        }
    }
}
