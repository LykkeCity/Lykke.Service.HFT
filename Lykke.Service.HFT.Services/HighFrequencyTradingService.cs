using System;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.HFT.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Lykke.Service.HFT.Services
{
    public class HighFrequencyTradingService : IHighFrequencyTradingService
	{
		private readonly IDistributedCache _distributedCache;
		private readonly IMatchingEngineClient _matchingEngineClient;

		public HighFrequencyTradingService(IMatchingEngineClient matchingEngineClient, IDistributedCache distributedCache)
		{
			_matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
			_distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
		}

		public bool IsConnected => _matchingEngineClient.IsConnected;
	}
}
