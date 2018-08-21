using System;

namespace Lykke.Service.HFT.Core.Settings
{
    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }
        public TimeSpan CacheExpirationPeriod { get; set; }
    }
}