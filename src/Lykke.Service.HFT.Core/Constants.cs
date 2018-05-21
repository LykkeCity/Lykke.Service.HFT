using System;

namespace Lykke.Service.HFT.Core
{
    public static class Constants
    {
        public const string ComponentName = "HFT API (high-frequency-trading-service)";

        public static ulong OrderCounter;
        public static TimeSpan TotalProcessingTime = TimeSpan.Zero;
        public static TimeSpan AssetPairTime = TimeSpan.Zero;
        public static TimeSpan ValidationTime = TimeSpan.Zero;
        public static TimeSpan MongoTime = TimeSpan.Zero;
        public static TimeSpan FeeTime = TimeSpan.Zero;
        public static TimeSpan MeTime = TimeSpan.Zero;
    }
}
