using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using CancelMode = Lykke.Service.HFT.Contracts.Orders.CancelMode;
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

            return ConvertToApiModel(response.Status);
        }

        public async Task<ResponseModel<MarketOrderResponseModel>> PlaceMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume,
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

            var result = new MarketOrderResponseModel
            {
                Price = response.Price
            };

            return ConvertToApiModel(response.Status, result);
        }

        public async Task<ResponseModel<LimitOrderResponseModel>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume,
            double price, bool cancelPreviousOrders = false)
        {
            var requestId = await StoreLimitOrder(clientId, assetPair, volume, price);

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

            var result = new LimitOrderResponseModel
            {
                Id = requestId
            };

            return ConvertToApiModel(response.Status, result);
        }

        private async Task<Guid> StoreLimitOrder(string clientId, AssetPair assetPair, double volume, double price)
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
            return requestId;
        }

        public async Task<ResponseModel<BulkOrderResponseModel>> PlaceBulkLimitOrderAsync(string clientId, AssetPair assetPair, IEnumerable<BulkOrderItemModel> items, bool cancelPrevious, CancelMode? cancelMode)
        {
            var requestId = GetNextRequestId();

            var orders = new ConcurrentBag<MultiOrderItemModel>();
            await items.ParallelForEachAsync(async item =>
            {
                var subOrder = await ToMultiOrderItemModel(clientId, assetPair, item);
                orders.Add(subOrder);
            });

            var order = new MultiLimitOrderModel
            {
                Id = requestId.ToString(),
                ClientId = clientId,
                AssetId = assetPair.Id,
                CancelPreviousOrders = cancelPrevious,
                Orders = orders.ToArray()
            };

            if (cancelMode.HasValue)
            {
                order.CancelMode = cancelMode.Value.ToMeCancelModel();
            }

            var response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(order);
            await CheckResponseAndThrowIfNull(response);

            var result = new BulkOrderResponseModel
            {
                AssetPairId = assetPair.Id,
                Error = response.Status != MeStatusCodes.Ok ? GetErrorCodeType(response.Status) : default(ErrorCodeType?),
                Statuses = response.Statuses?.Select(ToBulkOrderItemStatusModel).ToArray()
            };

            return ConvertToApiModel(response.Status, result);
        }

        private BulkOrderItemStatusModel ToBulkOrderItemStatusModel(LimitOrderStatusModel status)
        {
            Guid.TryParse(status.Id, out var id);

            return new BulkOrderItemStatusModel
            {
                Id = id,
                Error = status.Status != MeStatusCodes.Ok ? GetErrorCodeType(status.Status) : default(ErrorCodeType?),
                Price = status.Price,
                Volume = status.Volume
            };
        }

        private async Task<MultiOrderItemModel> ToMultiOrderItemModel(string clientId, AssetPair assetPair, BulkOrderItemModel item)
        {
            var itemId = await StoreLimitOrder(clientId, assetPair, item.Volume, item.Price);
            var fees = await _feeCalculator.GetLimitOrderFees(clientId, assetPair, item.OrderAction);

            var model = new MultiOrderItemModel
            {
                Id = itemId.ToString(),
                Price = item.Price,
                Volume = item.Volume,
                OrderAction = item.OrderAction.ToMeOrderAction(),
                Fee = fees.FirstOrDefault()
            };

            if (!string.IsNullOrWhiteSpace(item.OldId))
            {
                model.OldId = item.OldId;
            }

            return model;
        }

        private static Guid GetNextRequestId() => Guid.NewGuid();

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
                    _log.WriteWarning(nameof(MatchingEngineAdapter), nameof(GetErrorCodeType), $"Unknown ME status code {code}");
                    return ErrorCodeType.Runtime;
            }
        }
    }
}
