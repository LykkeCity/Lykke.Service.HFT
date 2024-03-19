using Autofac;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.HFT.Services.Consumers;
using Lykke.SettingsReader;

namespace Lykke.Service.HFT.Modules
{
    internal class RabbitMqModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ClientSettingsUpdatesConsumer>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.HighFrequencyTradingService.RabbitMq.ClientAccountFeedConnectionString))
                .AsSelf()
                .SingleInstance();
        }
    }
}
