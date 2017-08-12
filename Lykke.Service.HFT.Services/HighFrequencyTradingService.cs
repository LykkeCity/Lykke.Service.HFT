using System;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.HFT.Abstractions.Services;

namespace Lykke.Service.HFT.Services
{
    public class HighFrequencyTradingService : IHighFrequencyTradingService
	{

		private readonly IMatchingEngineClient _matchingEngineClient;

		public HighFrequencyTradingService(IMatchingEngineClient matchingEngineClient)
		{
			_matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
		}

		public bool IsConnected => _matchingEngineClient.IsConnected;
	}
}
