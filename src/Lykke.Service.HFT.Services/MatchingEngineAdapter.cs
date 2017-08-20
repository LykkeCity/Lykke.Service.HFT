﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using Lykke.Service.HFT.Services.Messages;
using OrderAction = Lykke.MatchingEngine.Connector.Abstractions.Models.OrderAction;

namespace Lykke.Service.HFT.Services
{
	public class MatchingEngineAdapter : IMatchingEngineAdapter
	{
		private readonly IMatchingEngineClient _matchingEngineClient;
		public static readonly ConcurrentDictionary<string, LimitOrderMessage.Order> LimitOrders = new ConcurrentDictionary<string, LimitOrderMessage.Order>();

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
			LimitOrders.TryAdd(id, new LimitOrderMessage.Order());
			var response = await _matchingEngineClient.PlaceLimitOrderAsync(id, clientId, assetPairId, (OrderAction)orderAction, volume, price, cancelPreviousOrders);
			if (response.Status == MeStatusCodes.Ok)
			{
				return ResponseModel<string>.CreateOk(id);
			}
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
