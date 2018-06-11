using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using OrderAction = Lykke.Service.HFT.Core.Domain.OrderAction;

namespace Lykke.Service.HFT.Services
{
    public class MatchingEngineAdapter : IMatchingEngineAdapter
    {
        private readonly ILog _log;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly ILimitOrderStateRepository _orderStateRepository;
        private readonly IFeeCalculatorAdapter _feeCalculator;

        public MatchingEngineAdapter(IMatchingEngineClient matchingEngineClient,
            ILimitOrderStateRepository orderStateRepository,
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

        public async Task<ResponseModel> CancelAllAsync(string clientId, AssetPair pair, bool? isBuy)
        {
            var model = new LimitOrderMassCancelModel
            {
                Id = GetNextRequestId().ToString(),
                AssetPairId = pair?.Id,
                ClientId = clientId,
                IsBuy = isBuy
            };

            var response = await _matchingEngineClient.MassCancelLimitOrdersAsync(model);
            await CheckResponseAndThrowIfNull(response);

            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel.CreateOk();
            }

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
            return status == MeStatusCodes.Ok
                ? ResponseModel.CreateOk()
                : ResponseModel.CreateFail(GetErrorCodeType(status));
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status)
        {
            return ResponseModel<T>.CreateFail(GetErrorCodeType(status));
        }

        private ResponseModel.ErrorCodeType GetErrorCodeType(MeStatusCodes code)
        {
            switch (code)
            {
                case MeStatusCodes.Ok:
                    throw new InvalidOperationException("Ok is not an error.");
                case MeStatusCodes.LowBalance:
                    return ResponseModel.ErrorCodeType.LowBalance;
                case MeStatusCodes.AlreadyProcessed:
                    return ResponseModel.ErrorCodeType.AlreadyProcessed;
                case MeStatusCodes.UnknownAsset:
                    return ResponseModel.ErrorCodeType.UnknownAsset;
                case MeStatusCodes.NoLiquidity:
                    return ResponseModel.ErrorCodeType.NoLiquidity;
                case MeStatusCodes.NotEnoughFunds:
                    return ResponseModel.ErrorCodeType.NotEnoughFunds;
                case MeStatusCodes.Dust:
                    return ResponseModel.ErrorCodeType.Dust;
                case MeStatusCodes.ReservedVolumeHigherThanBalance:
                    return ResponseModel.ErrorCodeType.ReservedVolumeHigherThanBalance;
                case MeStatusCodes.NotFound:
                    return ResponseModel.ErrorCodeType.NotFound;
                case MeStatusCodes.BalanceLowerThanReserved:
                    return ResponseModel.ErrorCodeType.BalanceLowerThanReserved;
                case MeStatusCodes.LeadToNegativeSpread:
                    return ResponseModel.ErrorCodeType.LeadToNegativeSpread;
                case MeStatusCodes.TooSmallVolume:
                    return ResponseModel.ErrorCodeType.Dust;
                case MeStatusCodes.InvalidFee:
                    return ResponseModel.ErrorCodeType.InvalidFee;
                case MeStatusCodes.Duplicate:
                    return ResponseModel.ErrorCodeType.Duplicate;
                case MeStatusCodes.Runtime:
                    return ResponseModel.ErrorCodeType.Runtime;
                case MeStatusCodes.BadRequest:
                    return ResponseModel.ErrorCodeType.BadRequest;
                case MeStatusCodes.InvalidPrice:
                    return ResponseModel.ErrorCodeType.InvalidPrice;
                case MeStatusCodes.Replaced:
                    return ResponseModel.ErrorCodeType.Replaced;
                case MeStatusCodes.NotFoundPrevious:
                    return ResponseModel.ErrorCodeType.NotFoundPrevious;
                default:
                    _log.WriteWarning(nameof(MatchingEngineAdapter), nameof(GetErrorCodeType), $"Unknown ME status code {code}");
                    return ResponseModel.ErrorCodeType.Runtime;
            }
        }
    }
}
