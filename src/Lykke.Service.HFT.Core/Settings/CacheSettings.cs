using Lykke.SettingsReader.Attributes;
using System;

namespace Lykke.Service.HFT.Core.Settings
{
    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }

        [Optional]
        public TimeSpan CacheExpirationPeriod { get; set; } = TimeSpan.FromMinutes(30);

        [Optional]
        public string ApiKeyCacheInstance { get; set; } = "HftApiCache";

        [Optional]
        public string OrderBooksCacheInstance { get; set; } = "FinanceDataCacheInstance";

        [Optional]
        public string OrderBooksCacheKeyPattern { get; set; } = ":OrderBooks:{0}_{1}__";
    }
}