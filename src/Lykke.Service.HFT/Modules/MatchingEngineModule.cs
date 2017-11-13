using System;
using Autofac;
using Common;
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

        public MatchingEngineModule(IReloadingManager<AppSettings.MatchingEngineSettings> settings, IReloadingManager<AppSettings.HighFrequencyTradingSettings> hftSettings)
        {
            _settings = settings;
            _hftSettings = hftSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var socketLog = new SocketLogDynamic(i => { }, str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

            builder.BindMeClient(_settings.CurrentValue.IpEndpoint.GetClientIpEndPoint(), socketLog);

            builder.RegisterType<MatchingEngineAdapter>()
                .As<IMatchingEngineAdapter>()
                .WithParameter(TypedParameter.From(_hftSettings.CurrentValue.Fees))
                .SingleInstance();
        }
    }
}
