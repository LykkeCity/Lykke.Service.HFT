using System;
using System.Collections.Generic;
using System.Net;
using Lykke.Common.Chaos;
using Lykke.Sdk.Settings;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.HFT.Core
{
    public class AppSettings : BaseAppSettings
    {
        public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public AssetsServiceClient AssetsServiceClient { get; set; }
        public BalancesServiceClient BalancesServiceClient { get; set; }
        public FeeCalculatorServiceClient FeeCalculatorServiceClient { get; set; }
        public OperationsHistoryServiceClientSettings OperationsHistoryServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }

        public class HighFrequencyTradingSettings
        {
            [Optional]
            public MaintenanceMode MaintenanceMode { get; set; }
            public List<string> DisabledAssets { get; set; }
            public DbSettings Db { get; set; }
            public DictionariesSettings Dictionaries { get; set; }
            public CacheSettings Cache { get; set; }
            public RabbitMqSettings LimitOrdersFeed { get; set; }
            public string QueuePostfix { get; set; }
            [AmqpCheck]
            public string CqrsRabbitConnString { get; set; }
            [Optional]
            public ChaosSettings ChaosKitty { get; set; }
        }

        public class MaintenanceMode
        {
            public bool Enabled { get; set; }
            public string Reason { get; set; }
        }

        public class RabbitMqSettings
        {
            [AmqpCheck]
            public string ConnectionString { get; set; }
            public string ExchangeName { get; set; }
        }

        public class MatchingEngineSettings
        {
            public IpEndpointSettings IpEndpoint { get; set; }
        }

        public class IpEndpointSettings
        {
            [TcpCheck("Port")]
            public string Host { get; set; }
            public int Port { get; set; }

            public IPEndPoint GetClientIpEndPoint()
            {
                if (IPAddress.TryParse(Host, out var ipAddress))
                    return new IPEndPoint(ipAddress, Port);

                var addresses = Dns.GetHostAddressesAsync(Host).Result;
                return new IPEndPoint(addresses[0], Port);
            }
        }

        public class DbSettings
        {
            [AzureBlobCheck]
            public string LogsConnString { get; set; }
            public string OrderStateConnString { get; set; }
            [AzureTableCheck]
            public string OrdersArchiveConnString { get; set; }
        }

        public class DictionariesSettings
        {
            public TimeSpan CacheExpirationPeriod { get; set; }
        }
    }

    public class CacheSettings
    {
        public string RedisConfiguration { get; set; }

        public string ApiKeyCacheInstance { get; set; }
        public string ApiKeyCacheKeyPattern { get; set; }
        public string WalletCacheKeyPattern { get; set; }

        public string FinanceDataCacheInstance { get; set; }
        public string OrderBooksCacheKeyPattern { get; set; }
    }

    public static class CacheSettingsExt
    {
        public static string GetKeyForApiKey(this CacheSettings settings, string apiKey)
        {
            return string.Format(settings.ApiKeyCacheKeyPattern, apiKey);
        }

        public static string GetKeyForWalletId(this CacheSettings settings, string wallet)
        {
            return string.Format(settings.WalletCacheKeyPattern, wallet);
        }

        public static string GetKeyForOrderBook(this CacheSettings settings, string assetPairId, bool isBuy)
        {
            return string.Format(settings.OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }

    public class FeeSettings
    {
        public TargetClientIdFeeSettings TargetClientId { get; set; }
    }

    public class TargetClientIdFeeSettings
    {
        public string Hft { get; set; }
    }

    public class AssetsServiceClient
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }

    public class BalancesServiceClient
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }

    public class FeeCalculatorServiceClient
    {
        [HttpCheck("api/IsAlive")]
        public string ServiceUrl { get; set; }
    }
}
