using Autofac;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.HFT.Core.Settings;
using Lykke.Service.History.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.HFT.Modules
{
    internal class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ClientsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssetsClient(_settings.CurrentValue.AssetsServiceClient.ServiceUrl);

            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient.ServiceUrl);

            builder.RegisterFeeCalculatorClientWithCache(
                _settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl,
                _settings.CurrentValue.HighFrequencyTradingService.Cache.CacheExpirationPeriod);

            builder.RegisterHistoryClient(_settings.CurrentValue.HistoryServiceClient);
        }
    }
}