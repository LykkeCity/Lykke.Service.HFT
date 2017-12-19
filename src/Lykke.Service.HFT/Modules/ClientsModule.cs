using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.SettingsReader;
using Lykke.Service.HFT.Core;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.OperationsHistory.Client;

namespace Lykke.Service.HFT.Modules
{
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly IServiceCollection _services;
        private readonly ILog _log;

        public ClientsModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient.ServiceUrl, _log);

            builder.RegisterFeeCalculatorClient(_settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl, _log);

            _services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl),
                _settings.CurrentValue.HighFrequencyTradingService.Dictionaries.CacheExpirationPeriod));

            builder.RegisterOperationsHistoryClient(_settings.CurrentValue.OperationsHistoryServiceClient, _log);

            builder.Populate(_services);
        }
    }
}