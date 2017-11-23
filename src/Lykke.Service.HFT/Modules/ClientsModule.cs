using Autofac;
using Common.Log;
using Lykke.Service.Balances.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.SettingsReader;
using Lykke.Service.HFT.Core;

namespace Lykke.Service.HFT.Modules
{
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ClientsModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient.ServiceUrl, _log);

            builder.RegisterFeeCalculatorClient(_settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl, _log);
        }
    }
}