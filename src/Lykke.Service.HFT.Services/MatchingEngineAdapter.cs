using System;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.HFT.Core.Services;

namespace Lykke.Service.HFT.Services
{
    public class MatchingEngineAdapter : IMatchingEngineAdapter
	{
		private readonly IMatchingEngineClient _matchingEngineClient;

		public MatchingEngineAdapter(IMatchingEngineClient matchingEngineClient)
		{
			_matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
		}

		public bool IsConnected => _matchingEngineClient.IsConnected;

		public async Task PlaceLimitOrderAsync(string clientId, string assetPairId, string orderAction, double volume, double price)
		{
			var orderId = Guid.NewGuid().ToString();
			OrderAction orderActionValue;
			Enum.TryParse(orderAction, out orderActionValue);
			var response = await _matchingEngineClient.PlaceLimitOrderAsync(
				id: orderId, 
				assetPairId: assetPairId, 
				clientId: clientId,
				orderAction: orderActionValue, 
				volume: volume, 
				price: price);
		}
	}
}
