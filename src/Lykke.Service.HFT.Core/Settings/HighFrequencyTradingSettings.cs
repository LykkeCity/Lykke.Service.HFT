using System.Collections.Generic;
using Lykke.Common.Chaos;
using Lykke.Sdk.Health;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class HighFrequencyTradingSettings
    {
        [Optional]
        public MaintenanceMode MaintenanceMode { get; set; }
        public List<string> DisabledAssets { get; set; }
        public DbSettings Db { get; set; }
        public CacheSettings Cache { get; set; }
        public RabbitMqSettings LimitOrdersFeed { get; set; }
        public string QueuePostfix { get; set; }
        [AmqpCheck]
        public string CqrsRabbitConnString { get; set; }
        [Optional]
        public ChaosSettings ChaosKitty { get; set; }
        [Optional]
        public bool CalculateOrderFees { get; set; }
    }
}
