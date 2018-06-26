using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using OrderAction = Lykke.Service.HFT.Contracts.Orders.OrderAction;

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

            if (response.Status == MeStatusCodes.Ok)
            {
                return ResponseModel.CreateOk();
            }

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

            await _orderStateRepository.Add(new LimitOrderState
            {
                Id = requestId,
                ClientId = clientId,
                AssetPairId = assetPair.Id,
                Volume = volume,
                Price = price,
                CreatedAt = DateTime.UtcNow
            });

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
            var errorCode = GetErrorCodeType(status);
            return status == MeStatusCodes.Ok
                ? ResponseModel.CreateOk()
                : ResponseModel.CreateFail(errorCode, errorCode.GetErrorMessage());
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status)
        {
            var errorCode = GetErrorCodeType(status);
            return ResponseModel<T>.CreateFail(errorCode, errorCode.GetErrorMessage());
        }

        private ErrorCodeType GetErrorCodeType(MeStatusCodes code)
        {
            switch (code)
            {
                case MeStatusCodes.Ok:
                    throw new InvalidOperationException("Ok is not an error.");
                case MeStatusCodes.LowBalance:
                    return ErrorCodeType.LowBalance;
                case MeStatusCodes.AlreadyProcessed:
                    return ErrorCodeType.AlreadyProcessed;
                case MeStatusCodes.UnknownAsset:
                    return ErrorCodeType.UnknownAsset;
                case MeStatusCodes.NoLiquidity:
                    return ErrorCodeType.NoLiquidity;
                case MeStatusCodes.NotEnoughFunds:
                    return ErrorCodeType.NotEnoughFunds;
                case MeStatusCodes.Dust:
                    return ErrorCodeType.Dust;
                case MeStatusCodes.ReservedVolumeHigherThanBalance:
                    return ErrorCodeType.ReservedVolumeHigherThanBalance;
                case MeStatusCodes.NotFound:
                    return ErrorCodeType.NotFound;
                case MeStatusCodes.BalanceLowerThanReserved:
                    return ErrorCodeType.BalanceLowerThanReserved;
                case MeStatusCodes.LeadToNegativeSpread:
                    return ErrorCodeType.LeadToNegativeSpread;
                case MeStatusCodes.TooSmallVolume:
                    return ErrorCodeType.Dust;
                case MeStatusCodes.InvalidFee:
                    return ErrorCodeType.InvalidFee;
                case MeStatusCodes.Duplicate:
                    return ErrorCodeType.Duplicate;
                case MeStatusCodes.Runtime:
                    return ErrorCodeType.Runtime;
                case MeStatusCodes.BadRequest:
                    return ErrorCodeType.BadRequest;
                case MeStatusCodes.InvalidPrice:
                    return ErrorCodeType.InvalidPrice;
                case MeStatusCodes.Replaced:
                    return ErrorCodeType.Replaced;
                case MeStatusCodes.NotFoundPrevious:
                    return ErrorCodeType.NotFoundPrevious;
                default:
                    _log.WriteWarning(nameof(MatchingEngineAdapter), nameof(GetErrorCodeType), $"Unknown ME status code {code}");
                    return ErrorCodeType.Runtime;
            }
        }
    }
}
