using Autofac;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services;
using Lykke.SettingsReader;
using System;
using Lykke.Service.HFT.Core.Settings;

namespace Lykke.Service.HFT.Modules
{
    internal class MatchingEngineModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public MatchingEngineModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisgterMeClient(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint());

            builder.RegisterType<MatchingEngineAdapter>()
                .As<IMatchingEngineAdapter>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.HighFrequencyTradingService.CalculateOrderFees))
                .SingleInstance();
        }
    }
}
