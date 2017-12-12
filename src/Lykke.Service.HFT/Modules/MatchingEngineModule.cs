using System;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services;
using Lykke.SettingsReader;

namespace Lykke.Service.HFT.Modules
{
    public class MatchingEngineModule : Module
    {
        private readonly IReloadingManager<AppSettings.MatchingEngineSettings> _settings;
        private readonly IReloadingManager<AppSettings.HighFrequencyTradingSettings> _hftSettings;
        private readonly ILog _log;

        public MatchingEngineModule(
            [NotNull] IReloadingManager<AppSettings.MatchingEngineSettings> settings,
            [NotNull] IReloadingManager<AppSettings.HighFrequencyTradingSettings> hftSettings,
            [NotNull] ILog log)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _hftSettings = hftSettings ?? throw new ArgumentNullException(nameof(hftSettings));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        protected override void Load(ContainerBuilder builder)
        {
            var socketLog = new SocketLogDynamic(i => { }, str => _log.WriteInfoAsync("Lykke.MatchingEngine.Connector", "IMatchingEngineClient", str).Wait());

            builder.BindMeClient(_settings.CurrentValue.IpEndpoint.GetClientIpEndPoint(), socketLog);

            builder.RegisterType<MatchingEngineAdapter>()
                .As<IMatchingEngineAdapter>()
                .WithParameter(TypedParameter.From(_hftSettings.CurrentValue.Fees))
                .SingleInstance();
        }
    }
}
