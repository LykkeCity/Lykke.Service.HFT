using System;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core.Settings
{
    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }

        [Optional]
        public TimeSpan CacheExpirationPeriod { get; set; } = TimeSpan.FromMinutes(30);
    }
}