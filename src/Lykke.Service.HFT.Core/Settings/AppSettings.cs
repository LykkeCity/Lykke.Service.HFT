using Lykke.Sdk.Settings;
using Lykke.Service.OperationsHistory.Client;

namespace Lykke.Service.HFT.Core.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public BalancesServiceClientSettings BalancesServiceClient { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public OperationsHistoryServiceClientSettings OperationsHistoryServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }
    }
}
