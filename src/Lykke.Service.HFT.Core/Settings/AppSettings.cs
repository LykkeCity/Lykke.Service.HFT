using Lykke.Sdk.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.History.Client;

namespace Lykke.Service.HFT.Core.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public BalancesServiceClientSettings BalancesServiceClient { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public HistoryServiceClientSettings HistoryServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
    }
}
