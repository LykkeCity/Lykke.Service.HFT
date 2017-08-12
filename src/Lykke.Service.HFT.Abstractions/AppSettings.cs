using System.Net;

namespace Lykke.Service.HFT.Abstractions
{
    public class AppSettings
	{
		public HighFrequencyTradingSettings HighFrequencyTradingService { get; set; }

	}

	public class HighFrequencyTradingSettings
	{
		public MatchingOrdersSettings MatchingEngine { get; set; }
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
}
