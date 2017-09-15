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
		private readonly IRepository<LimitOrderState> _orderStateRepository;

		private readonly Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType> _statusCodesMap = new Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType>
		{
			{MeStatusCodes.Ok, ResponseModel.ErrorCodeType.Ok},
			{MeStatusCodes.LowBalance, ResponseModel.ErrorCodeType.LowBalance},
			{MeStatusCodes.AlreadyProcessed, ResponseModel.ErrorCodeType.AlreadyProcessed},
			{MeStatusCodes.UnknownAsset, ResponseModel.ErrorCodeType.UnknownAsset},
			{MeStatusCodes.NoLiquidity, ResponseModel.ErrorCodeType.NoLiquidity},
			{MeStatusCodes.NotEnoughFunds, ResponseModel.ErrorCodeType.NotEnoughFunds},
			{MeStatusCodes.Dust, ResponseModel.ErrorCodeType.Dust},
			{MeStatusCodes.ReservedVolumeHigherThanBalance, ResponseModel.ErrorCodeType.ReservedVolumeHigherThanBalance},
			{MeStatusCodes.NotFound, ResponseModel.ErrorCodeType.NotFound},
			{MeStatusCodes.Runtime, ResponseModel.ErrorCodeType.RuntimeError}
		};


		public MatchingEngineAdapter(IMatchingEngineClient matchingEngineClient, IRepository<LimitOrderState> orderStateRepository)
		{
			_matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
			_orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
		}

		public bool IsConnected => _matchingEngineClient.IsConnected;

		public async Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId)
		{
			var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId.ToString());
			return ConvertToApiModel(response);
		}

		public async Task<ResponseModel> CashInOutAsync(string clientId, string assetId, double amount)
		{
			var id = GetNextRequestId();
			var response = await _matchingEngineClient.CashInOutAsync(id, clientId, assetId, amount);
			return ConvertToApiModel(response);
		}

		public async Task<ResponseModel<Guid>> HandleMarketOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			bool straight, double? reservedLimitVolume = null)
		{
			var requestId = GetNextRequestGuid();
			var matchingEngineResponse = await _matchingEngineClient.HandleMarketOrderAsync(requestId.ToString(), clientId, assetPairId, (OrderAction)orderAction, volume, straight, reservedLimitVolume);
		    if (!string.IsNullOrEmpty(matchingEngineResponse))
		    {
		        return ResponseModel<Guid>.CreateFail(ResponseModel.ErrorCodeType.RuntimeError, matchingEngineResponse);
		    }
		    return ResponseModel<Guid>.CreateOk(requestId);
        }

		public async Task<ResponseModel<Guid>> PlaceLimitOrderAsync(string clientId, string assetPairId, Core.Domain.OrderAction orderAction, double volume,
			double price, bool cancelPreviousOrders = false)
		{
			var requestId = GetNextRequestGuid();
			await _orderStateRepository.Add(new LimitOrderState { Id = requestId, ClientId = clientId, AssetPairId = assetPairId, Volume = volume, Price = price });
			var response = await _matchingEngineClient.PlaceLimitOrderAsync(requestId.ToString(), clientId, assetPairId, (OrderAction)orderAction, volume, price, cancelPreviousOrders);
			if (response.Status == MeStatusCodes.Ok)
			{
				return ResponseModel<Guid>.CreateOk(requestId);
			}
			return ConvertToApiModel<Guid>(response);
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

		private Guid GetNextRequestGuid()
		{
			return Guid.NewGuid();
		}
		private string GetNextRequestId()
		{
			return Guid.NewGuid().ToString();
		}

		private ResponseModel ConvertToApiModel(MeResponseModel response)
		{
			if (response.Status == MeStatusCodes.Ok)
				return ResponseModel.CreateOk();

			return ResponseModel.CreateFail(_statusCodesMap[response.Status]);
		}

		private ResponseModel<T> ConvertToApiModel<T>(MeResponseModel response)
		{
			return ResponseModel<T>.CreateFail(_statusCodesMap[response.Status]);
		}
	}
}
