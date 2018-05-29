using Autofac;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services;
using Lykke.SettingsReader;
using System;

namespace Lykke.Service.HFT.Modules
{
    public class MatchingEngineModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public MatchingEngineModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.BindMeClient(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint(), socketLog: null, ignoreErrors: true);

            builder.RegisterType<MatchingEngineAdapter>()
                .As<IMatchingEngineAdapter>()
                .SingleInstance();
        }
    }
}
