using System;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using OrderAction = Lykke.MatchingEngine.Connector.Abstractions.Models.OrderAction;

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

		public async Task<ResponseModel> CancelLimitOrderAsync(string limitOrderId)
		{
			var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId);
			return ConvertToApiModel(response);
		}

		public async Task<ResponseModel> CashInOutAsync(string clientId, string assetId, double amount)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.CashInOutAsync(id, clientId, assetId, amount);
			return ConvertToApiModel(response);
		}

		public async Task HandleMarketOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			bool straight, double? reservedLimitVolume = null)
		{
			var id = GetNextRequestId();
			var matchingEngineId = await _matchingEngineClient.HandleMarketOrderAsync(id, clientId, assetPairId, (OrderAction)orderAction, volume, straight, reservedLimitVolume);
		}

		public async Task<ResponseModel<string>> PlaceLimitOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			double price, bool cancelPreviousOrders = false)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.PlaceLimitOrderAsync(id, clientId, assetPairId, (OrderAction)orderAction, volume, price, cancelPreviousOrders);
			if (response.Status == MeStatusCodes.Ok)
				return ResponseModel<string>.CreateOk(id);
			return ConvertToApiModel<string>(response);
		}

		public async Task<ResponseModel> SwapAsync(string clientId1, string assetId1, double amount1, string clientId2, string assetId2, double amount2)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.SwapAsync(id, clientId1, assetId1, amount1, clientId2, assetId2, amount2);
			return ConvertToApiModel(response);
		}

		public async Task<ResponseModel> TransferAsync(string fromClientId, string toClientId, string assetId, double amount)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.TransferAsync(id, fromClientId, toClientId, assetId, amount);
			return ConvertToApiModel(response);
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

		private ResponseModel ConvertToApiModel(MeResponseModel response)
		{
			switch (response.Status)
			{
				case MeStatusCodes.Ok:
					return new ResponseModel { Status = StatusCodes.Ok };
				case MeStatusCodes.LowBalance:
					return new ResponseModel { Status = StatusCodes.LowBalance, Message = "Low balance" };
				case MeStatusCodes.AlreadyProcessed:
					return new ResponseModel { Status = StatusCodes.AlreadyProcessed, Message = "Already Processed" };
				case MeStatusCodes.UnknownAsset:
					return new ResponseModel { Status = StatusCodes.UnknownAsset, Message = "Unknown asset" };
				case MeStatusCodes.NoLiquidity:
					return new ResponseModel { Status = StatusCodes.NoLiquidity, Message = "No liquidity" };
				case MeStatusCodes.NotEnoughFunds:
					return new ResponseModel { Status = StatusCodes.NotEnoughFunds, Message = "Not enough funds" };
				case MeStatusCodes.Dust:
					return new ResponseModel { Status = StatusCodes.Dust, Message = "Dust" };
				case MeStatusCodes.ReservedVolumeHigherThanBalance:
					return new ResponseModel { Status = StatusCodes.ReservedVolumeHigherThanBalance, Message = "Reserved volume higher than balance" };
				case MeStatusCodes.NotFound:
					return new ResponseModel { Status = StatusCodes.NotFound, Message = "Not found" };
				case MeStatusCodes.Runtime:
					return new ResponseModel { Status = StatusCodes.Runtime, Message = "Runtime error" };
				default:
					return new ResponseModel { Status = StatusCodes.Runtime, Message = "Runtime error" };
			}
		}
		private ResponseModel<T> ConvertToApiModel<T>(MeResponseModel response)
		{
			switch (response.Status)
			{
				case MeStatusCodes.Ok:
					return new ResponseModel<T> { Status = StatusCodes.Ok };
				case MeStatusCodes.LowBalance:
					return new ResponseModel<T> { Status = StatusCodes.LowBalance, Message = "Low balance" };
				case MeStatusCodes.AlreadyProcessed:
					return new ResponseModel<T> { Status = StatusCodes.AlreadyProcessed, Message = "Already Processed" };
				case MeStatusCodes.UnknownAsset:
					return new ResponseModel<T> { Status = StatusCodes.UnknownAsset, Message = "Unknown asset" };
				case MeStatusCodes.NoLiquidity:
					return new ResponseModel<T> { Status = StatusCodes.NoLiquidity, Message = "No liquidity" };
				case MeStatusCodes.NotEnoughFunds:
					return new ResponseModel<T> { Status = StatusCodes.NotEnoughFunds, Message = "Not enough funds" };
				case MeStatusCodes.Dust:
					return new ResponseModel<T> { Status = StatusCodes.Dust, Message = "Dust" };
				case MeStatusCodes.ReservedVolumeHigherThanBalance:
					return new ResponseModel<T> { Status = StatusCodes.ReservedVolumeHigherThanBalance, Message = "Reserved volume higher than balance" };
				case MeStatusCodes.NotFound:
					return new ResponseModel<T> { Status = StatusCodes.NotFound, Message = "Not found" };
				case MeStatusCodes.Runtime:
					return new ResponseModel<T> { Status = StatusCodes.Runtime, Message = "Runtime error" };
				default:
					return new ResponseModel<T> { Status = StatusCodes.Runtime, Message = "Runtime error" };
			}
		}
	}
}
