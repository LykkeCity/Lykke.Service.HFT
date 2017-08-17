using System.Net;

namespace Lykke.Service.HFT.Core
{
    public class AppSettings
	{
		public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }

	}

	public class HighFrequencyTradingSettings
	{
		public MatchingOrdersSettings MatchingEngine { get; set; }
		public DbSettings Db { get; set; }
		public CacheSettings CacheSettings { get; set; }
	}
	public class MatchingOrdersSettings
	{
		public IpEndpointSettings IpEndpoint { get; set; }
	}

	public class IpEndpointSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }

		public IPEndPoint GetClientIpEndPoint(bool useInternal = false)
		{
			return new IPEndPoint(IPAddress.Parse(Host), Port);
		}
	}

	public class DbSettings
	{
		public string DictsConnString { get; set; }
		public string BalancesInfoConnString { get; set; }
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

}
