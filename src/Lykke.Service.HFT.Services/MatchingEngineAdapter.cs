using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Repositories;
using Lykke.Service.HFT.Core.Services;
using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ResponseModel<MarketOrderResponseModel>> PlaceMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, decimal volume,
            bool straight, double? reservedLimitVolume = null)
        {
            var order = new MarketOrderModel
            {
                Id = GetNextRequestId().ToString(),
                AssetPairId = assetPair.Id,
                ClientId = clientId,
                ReservedLimitVolume = reservedLimitVolume,
                Straight = straight,
                Volume = (double)Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fees = await _feeCalculator.GetMarketOrderFees(clientId, assetPair, orderAction)
            };

            var response = await _matchingEngineClient.HandleMarketOrderAsync(order);
            CheckResponseAndThrowIfNull(response);

            var result = new MarketOrderResponseModel
            {
                Price = response.Price
            };

            return ConvertToApiModel(response.Status, result);
        }

        public async Task<ResponseModel<LimitOrderResponseModel>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, decimal volume,
            decimal price, bool cancelPreviousOrders = false)
        {
            var requestId = await StoreLimitOrder(clientId, assetPair, volume, LimitOrderType.Default, x => x.Price = price);

            var order = new LimitOrderModel
            {
                Id = requestId.ToString(),
                AssetPairId = assetPair.Id,
                ClientId = clientId,
                Price = (double)price,
                CancelPreviousOrders = cancelPreviousOrders,
                Volume = (double)Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fees = await _feeCalculator.GetLimitOrderFees(clientId, assetPair, orderAction)
            };

            var response = await _matchingEngineClient.PlaceLimitOrderAsync(order);
            CheckResponseAndThrowIfNull(response);

            var result = new LimitOrderResponseModel
            {
                Id = requestId
            };

            return ConvertToApiModel(response.Status, result);
        }

        private async Task<Guid> StoreLimitOrder(string clientId, AssetPair assetPair, decimal volume, 
            LimitOrderType type, Action<LimitOrderState> setPrice)
        {
            var requestId = GetNextRequestId();

            var order = new LimitOrderState
            {
                Id = requestId,
                ClientId = clientId,
                AssetPairId = assetPair.Id,
                Volume = volume,
                CreatedAt = DateTime.UtcNow,
                Type = (int)type
            };
            setPrice(order);

            await _orderStateRepository.Add(order);
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
                AssetPairId = assetPair.Id,
                CancelPreviousOrders = cancelPrevious,
                Orders = orders.ToArray()
            };

            if (cancelMode.HasValue)
            {
                order.CancelMode = cancelMode.Value.ToMeCancelModel();
            }

            var response = await _matchingEngineClient.PlaceMultiLimitOrderAsync(order);
            CheckResponseAndThrowIfNull(response);

            var result = new BulkOrderResponseModel
            {
                AssetPairId = assetPair.Id,
                Error = response.Status != MeStatusCodes.Ok ? ErrorCodeType.Rejected : default(ErrorCodeType?),
                Statuses = response.Statuses?.Select(ToBulkOrderItemStatusModel).ToArray()
            };

            return ConvertToApiModel(response.Status, result);
        }

        public async Task<ResponseModel<LimitOrderResponseModel>> PlaceStopLimitOrderAsync(
            string clientId, AssetPair assetPair, OrderAction orderAction, decimal volume,
            decimal? lowerPrice, decimal? lowerLimitPrice, decimal? upperPrice, decimal? upperLimitPrice, bool cancelPreviousOrders = false)
        {
            _log.Info($"SEND {assetPair.Id} {lowerPrice} {lowerLimitPrice} {upperPrice} {upperLimitPrice}");

            var requestId = await StoreLimitOrder(clientId, assetPair, volume, LimitOrderType.Stop, x =>
                {
                    x.LowerPrice = lowerPrice;
                    x.LowerLimitPrice = lowerLimitPrice;
                    x.UpperPrice = upperPrice;
                    x.UpperLimitPrice = upperLimitPrice;
                });

            var order = new StopLimitOrderModel
            {
                Id = requestId.ToString(),
                AssetPairId = assetPair.Id,
                ClientId = clientId,
                LowerPrice = (double?)lowerPrice,
                LowerLimitPrice = (double?)lowerLimitPrice,
                UpperPrice = (double?)upperPrice,
                UpperLimitPrice = (double?)upperLimitPrice,
                CancelPreviousOrders = cancelPreviousOrders,
                Volume = (double)Math.Abs(volume),
                OrderAction = orderAction.ToMeOrderAction(),
                Fees = await _feeCalculator.GetLimitOrderFees(clientId, assetPair, orderAction)
            };

            var response = await _matchingEngineClient.PlaceStopLimitOrderAsync(order);
            CheckResponseAndThrowIfNull(response);

            var result = new LimitOrderResponseModel
            {
                Id = requestId
            };

            return ConvertToApiModel(response.Status, result);
        }

        private BulkOrderItemStatusModel ToBulkOrderItemStatusModel(LimitOrderStatusModel status)
        {
            Guid.TryParse(status.Id, out var id);

            return new BulkOrderItemStatusModel
            {
                Id = id,
                Error = status.Status != MeStatusCodes.Ok ? ErrorCodeType.Rejected : default(ErrorCodeType?),
                Price = (decimal)status.Price,
                Volume = (decimal)status.Volume
            };
        }

        private async Task<MultiOrderItemModel> ToMultiOrderItemModel(string clientId, AssetPair assetPair, BulkOrderItemModel item)
        {
            var itemId = await StoreLimitOrder(clientId, assetPair, item.Volume, LimitOrderType.Default, x => x.Price = item.Price);
            var fees = await _feeCalculator.GetLimitOrderFees(clientId, assetPair, item.OrderAction);

            var model = new MultiOrderItemModel
            {
                Id = itemId.ToString(),
                Price = (double)item.Price,
                Volume = (double)item.Volume,
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
                : CreateFail(x => ResponseModel.CreateFail(x, x.ToString()));
        }

        private ResponseModel<T> ConvertToApiModel<T>(MeStatusCodes status, T result)
        {
            if (status == MeStatusCodes.Ok)
            {
                return ResponseModel<T>.CreateOk(result);
            }

            var response = CreateFail(x => ResponseModel<T>.CreateFail(x, x.ToString()));
            response.Result = result;
            return response;
        }

        private T CreateFail<T>(Func<ErrorCodeType, T> creator)
        {
            var errorCode = ErrorCodeType.Rejected;
            return creator(errorCode);
        }
    }
}
