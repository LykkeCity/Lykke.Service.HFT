using System;
using System.Net;

namespace Lykke.Service.HFT.Core
{
    public class AppSettings
    {
        public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ExchangeSettings Exchange { get; set; }

        public class HighFrequencyTradingSettings
        {
            public string ApiKey { get; set; }
            public DbSettings Db { get; set; }
            public DictionariesSettings Dictionaries { get; set; }
            public CacheSettings CacheSettings { get; set; }
            public RabbitMqSettings LimitOrdersFeed { get; set; }
            public MongoSettings MongoSettings { get; set; }
            public RateLimitSettings.RateLimitCoreOptions IpRateLimiting { get; set; }
        }

        public class MongoSettings
        {
            public string ConnectionString { get; set; }
        }
        public class RabbitMqSettings
        {
            public string ConnectionString { get; set; }
            public string ExchangeName { get; set; }
        }
        public class MatchingEngineSettings
        {
            public IpEndpointSettings IpEndpoint { get; set; }
        }

        public class IpEndpointSettings
        {
            public string InternalHost { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }

            public IPEndPoint GetClientIpEndPoint(bool useInternal = false)
            {
                string host = useInternal ? InternalHost : Host;

                IPAddress ipAddress;
                if (IPAddress.TryParse(host, out ipAddress))
                    return new IPEndPoint(ipAddress, Port);

                var addresses = Dns.GetHostAddressesAsync(host).Result;
                return new IPEndPoint(addresses[0], Port);
            }
        }

        public class DbSettings
        {
            public string LogsConnString { get; set; }
            public string BalancesInfoConnString { get; set; }
        }

        public class DictionariesSettings
        {
            public string AssetsServiceUrl { get; set; }
            public TimeSpan CacheExpirationPeriod { get; set; }
        }
    }

    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }

        public string ApiKeyCacheInstance { get; set; }
        public string ApiKeyCacheKeyPattern { get; set; }

        public string FinanceDataCacheInstance { get; set; }
        public string OrderBooksCacheKeyPattern { get; set; }
    }

    public static class CacheSettingsExt
    {
        public static string GetApiKey(this CacheSettings settings, string apiKey)
        {
            return string.Format(settings.ApiKeyCacheKeyPattern, apiKey);
        }

        public static string GetOrderBookKey(this CacheSettings settings, string assetPairId, bool isBuy)
        {
            return string.Format(settings.OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }

    }

    public class SlackNotificationsSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
    }

    public class AzureQueueSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class ExchangeSettings
    {
        public decimal MaxLimitOrderDeviationPercent { get; set; }
    }
}
