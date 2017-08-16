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

		public async Task CancelLimitOrderAsync(string limitOrderId)
		{
			var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId);
			ValidateResponse(response);
		}

		public async Task CashInOutAsync(string clientId, string assetId, double amount)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.CashInOutAsync(id, clientId, assetId, amount);
			ValidateResponse(response);
		}
		
		public async Task HandleMarketOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			bool straight, double? reservedLimitVolume = null)
		{
			var id = GetNextRequestId();
			var matchingEngineId = await _matchingEngineClient.HandleMarketOrderAsync(id, clientId, assetPairId, (OrderAction)orderAction, volume, straight, reservedLimitVolume);
		}

		public async Task PlaceLimitOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			double price, bool cancelPreviousOrders = false)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.PlaceLimitOrderAsync(id, clientId, assetPairId, (OrderAction)orderAction, volume, price, cancelPreviousOrders);
			ValidateResponse(response);
		}

		public async Task SwapAsync(string clientId1, string assetId1, double amount1, string clientId2, string assetId2, double amount2)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.SwapAsync(id, clientId1, assetId1, amount1, clientId2, assetId2, amount2);
			ValidateResponse(response);
		}

		public async Task TransferAsync(string fromClientId, string toClientId, string assetId, double amount)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.TransferAsync(id, fromClientId, toClientId, assetId, amount);
			ValidateResponse(response);
		}

		public async Task UpdateBalanceAsync(string clientId, string assetId, double value)
		{
			var id = GetNextRequestId();
			await _matchingEngineClient.UpdateBalanceAsync(id, clientId, assetId, value);
		}

		private string GetNextRequestId()
		{
			return Guid.NewGuid().ToString();
		}


		// todo: check 'assetId' and 'assetPairId' for existence in every method 
		// todo: raise exception for global handling
		private void ValidateResponse(MeResponseModel response)
		{
			//if(response.Status != MeStatusCodes.Ok)
			//	throw new Exception();
		}
	}
}
