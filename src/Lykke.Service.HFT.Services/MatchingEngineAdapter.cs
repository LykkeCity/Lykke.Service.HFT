using System;
using System.Collections.Generic;
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
		private readonly Dictionary<MeStatusCodes, StatusCodes> _statusCodesMap = new Dictionary<MeStatusCodes, StatusCodes>
		{
			{MeStatusCodes.Ok, StatusCodes.Ok},
			{MeStatusCodes.LowBalance, StatusCodes.LowBalance},
			{MeStatusCodes.AlreadyProcessed, StatusCodes.AlreadyProcessed},
			{MeStatusCodes.UnknownAsset, StatusCodes.UnknownAsset},
			{MeStatusCodes.NoLiquidity, StatusCodes.NoLiquidity},
			{MeStatusCodes.NotEnoughFunds, StatusCodes.NotEnoughFunds},
			{MeStatusCodes.Dust, StatusCodes.Dust},
			{MeStatusCodes.ReservedVolumeHigherThanBalance, StatusCodes.ReservedVolumeHigherThanBalance},
			{MeStatusCodes.NotFound, StatusCodes.NotFound},
			{MeStatusCodes.Runtime, StatusCodes.RuntimeError}
		};


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
			return new ResponseModel { Status = _statusCodesMap[response.Status] };
		}

		private ResponseModel<T> ConvertToApiModel<T>(MeResponseModel response)
		{
			return new ResponseModel<T> { Status = _statusCodesMap[response.Status] };
		}
	}
}
