using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
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
            ILogFactory logFactory)
        {
            _matchingEngineClient = matchingEngineClient ?? throw new ArgumentNullException(nameof(matchingEngineClient));
            _orderStateRepository = orderStateRepository ?? throw new ArgumentNullException(nameof(orderStateRepository));
            _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));

            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            _log = logFactory.CreateLog(this);
        }

        public async Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId)
        {
            var response = await _matchingEngineClient.CancelLimitOrderAsync(limitOrderId.ToString());
            CheckResponseAndThrowIfNull(response);

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
            CheckResponseAndThrowIfNull(response);

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
            CheckResponseAndThrowIfNull(response);

            return ConvertToApiModel(response.Status, response.Price);
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
            CheckResponseAndThrowIfNull(response);

            return ConvertToApiModel(response.Status, requestId);
        }

        private static Guid GetNextRequestId() => Guid.NewGuid();

        private void CheckResponseAndThrowIfNull(object response)
        {
            if (response == null)
            {
                var exception = new InvalidOperationException("ME not available");
                _log.Error(exception, process: nameof(CancelLimitOrderAsync));
                throw exception;
            }
        }

        private ResponseModel ConvertToApiModel(MeStatusCodes status)
        {
            return status == MeStatusCodes.Ok
                ? ResponseModel.CreateOk()
                : CreateFail(status, x => ResponseModel.CreateFail(x, x.GetErrorMessage()));
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status, T result)
        {
            if (status == MeStatusCodes.Ok)
            {
                return ResponseModel<T>.CreateOk(result);
            }

            var response = CreateFail(status, x => ResponseModel<T>.CreateFail(x, x.GetErrorMessage()));
            response.Result = result;
            return response;
        }

        private T CreateFail<T>(MeStatusCodes status, Func<ErrorCodeType, T> creator)
        {
            var errorCode = GetErrorCodeType(status);
            return creator(errorCode);
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
                    _log.Warning($"Unknown ME status code {code}", process: nameof(GetErrorCodeType));
                    return ErrorCodeType.Runtime;
            }
        }
    }
}
