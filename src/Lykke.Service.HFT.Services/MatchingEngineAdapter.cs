﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using OrderAction = Lykke.Service.HFT.Core.Domain.OrderAction;

namespace Lykke.Service.HFT.Services
{
    public class MatchingEngineAdapter : IMatchingEngineAdapter
    {
        private readonly ILog _log;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IRepository<LimitOrderState> _orderStateRepository;
        private readonly IFeeCalculatorAdapter _feeCalculator;

        private static readonly Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType> StatusCodesMap = new Dictionary<MeStatusCodes, ResponseModel.ErrorCodeType>
        {
            {MeStatusCodes.LowBalance, ResponseModel.ErrorCodeType.LowBalance},
            {MeStatusCodes.AlreadyProcessed, ResponseModel.ErrorCodeType.AlreadyProcessed},
            {MeStatusCodes.UnknownAsset, ResponseModel.ErrorCodeType.UnknownAsset},
            {MeStatusCodes.NoLiquidity, ResponseModel.ErrorCodeType.NoLiquidity},
            {MeStatusCodes.NotEnoughFunds, ResponseModel.ErrorCodeType.NotEnoughFunds},
            {MeStatusCodes.Dust, ResponseModel.ErrorCodeType.Dust},
            {MeStatusCodes.ReservedVolumeHigherThanBalance, ResponseModel.ErrorCodeType.ReservedVolumeHigherThanBalance},
            {MeStatusCodes.NotFound, ResponseModel.ErrorCodeType.NotFound},
            {MeStatusCodes.BalanceLowerThanReserved, ResponseModel.ErrorCodeType.BalanceLowerThanReserved},
            {MeStatusCodes.LeadToNegativeSpread, ResponseModel.ErrorCodeType.LeadToNegativeSpread},
            {MeStatusCodes.TooSmallVolume, ResponseModel.ErrorCodeType.Dust},
            {MeStatusCodes.InvalidFee, ResponseModel.ErrorCodeType.Runtime},
            {MeStatusCodes.Duplicate, ResponseModel.ErrorCodeType.Runtime},
            {MeStatusCodes.Runtime, ResponseModel.ErrorCodeType.Runtime}
        };

        public MatchingEngineAdapter(IMatchingEngineClient matchingEngineClient,
            IRepository<LimitOrderState> orderStateRepository,
            IFeeCalculatorAdapter feeCalculator,
            [NotNull] ILog log)
        {
            _matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId)
        {
            var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId.ToString());
            await CheckResponseAndThrowIfNull(response);

            return ConvertToApiModel(response.Status);
        }

        public async Task<ResponseModel<double>> HandleMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume,
            bool straight, double? reservedLimitVolume = null)
        {
            var order = new MarketOrderModel
            {
                Id = GetNextRequestId().ToString(),
                AssetPairId = assetPair.Id,
                ClientId = clientId,
                ReservedLimitVolume = reservedLimitVolume,
                Straight = straight,
                Volume = Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fees = await _feeCalculator.GetMarketOrderFees(clientId, assetPair, orderAction)
            };

            var response = await _matchingEngineClient.HandleMarketOrderAsync(order);
            await CheckResponseAndThrowIfNull(response);
            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel<double>.CreateOk(response.Price);
            }
            return ConvertToApiModel<double>(response.Status);
        }

        public async Task<ResponseModel<Guid>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume,
            double price, bool cancelPreviousOrders = false)
        {
            var requestId = GetNextRequestId();

            await _orderStateRepository.Add(new LimitOrderState { Id = requestId, ClientId = clientId, AssetPairId = assetPair.Id, Volume = volume, Price = price });

            var order = new LimitOrderModel
            {
                Id = requestId.ToString(),
                AssetPairId = assetPair.Id,
                ClientId = clientId,
                Price = price,
                CancelPreviousOrders = cancelPreviousOrders,
                Volume = Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fees = await _feeCalculator.GetLimitOrderFees(clientId, assetPair, orderAction)
            };

            var response = await _matchingEngineClient.PlaceLimitOrderAsync(order);
            await CheckResponseAndThrowIfNull(response);
            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel<Guid>.CreateOk(requestId);
            }

            var responseModel = ConvertToApiModel<Guid>(response.Status);
            responseModel.Result = requestId;
            return responseModel;
        }

        private Guid GetNextRequestId()
        {
            return Guid.NewGuid();
        }

        private async Task CheckResponseAndThrowIfNull(object response)
        {
            if (response == null)
            {
                var exception = new InvalidOperationException("ME not available");
                await _log.WriteErrorAsync(nameof(MatchingEngineAdapter), nameof(CancelLimitOrderAsync), exception);
                throw exception;
            }
        }

        private ResponseModel ConvertToApiModel(MeStatusCodes status)
        {
            if (status == MeStatusCodes.Ok)
                return ResponseModel.CreateOk();

            return ResponseModel.CreateFail(StatusCodesMap[status]);
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status)
        {
            return ResponseModel<T>.CreateFail(StatusCodesMap[status]);
        }
    }
}
