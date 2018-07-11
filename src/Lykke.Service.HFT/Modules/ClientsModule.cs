using System;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.HFT.Core;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.HFT.Modules
{
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ClientsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl),
                _settings.CurrentValue.HighFrequencyTradingService.Dictionaries.CacheExpirationPeriod));

            var logWrapper = new LogWrapper(builder);

            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient.ServiceUrl, logWrapper);

            builder.RegisterFeeCalculatorClientWithCache(
                _settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl,
                _settings.CurrentValue.HighFrequencyTradingService.Dictionaries.CacheExpirationPeriod,
                logWrapper);

            builder.RegisterOperationsHistoryClient(_settings.CurrentValue.OperationsHistoryServiceClient, logWrapper);
        }

        // TODO Remove when all client libraries support new logging framework
        private class LogWrapper : ILog
        {
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable 618
            private ILog _log;

            public LogWrapper(ContainerBuilder builder)
            {
                builder.RegisterBuildCallback(x =>
                {
                    var factory = x.Resolve<ILogFactory>();
                    _log = factory.CreateLog(this);
                });
            }

            void ILog.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                _log?.Log(logLevel, eventId, state, exception, formatter);
            }

            bool ILog.IsEnabled(LogLevel logLevel)
                => _log?.IsEnabled(logLevel) ?? false;

            IDisposable ILog.BeginScope(string scopeMessage)
                => _log?.BeginScope(scopeMessage);

            Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
                => _log?.WriteInfoAsync(component, process, context, info, dateTime);

            Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
                => _log?.WriteMonitorAsync(component, process, context, info, dateTime);

            Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
                => _log?.WriteWarningAsync(component, process, context, info, dateTime);

            Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime)
                => _log?.WriteWarningAsync(component, process, context, info, ex, dateTime);

            Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
                => _log?.WriteErrorAsync(component, process, context, exception, dateTime);

            Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
                => _log?.WriteFatalErrorAsync(component, process, context, exception, dateTime);

            Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
                => _log?.WriteInfoAsync(process, context, info, dateTime);

            Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
                => _log?.WriteMonitorAsync(process, context, info, dateTime);

            Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
                => _log?.WriteWarningAsync(process, context, info, dateTime);

            Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
                => _log?.WriteWarningAsync(process, context, info, ex, dateTime);

            Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
                => _log?.WriteErrorAsync(process, context, exception, dateTime);

            Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
                => _log?.WriteFatalErrorAsync(process, context, exception, dateTime);
#pragma warning restore 618
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}