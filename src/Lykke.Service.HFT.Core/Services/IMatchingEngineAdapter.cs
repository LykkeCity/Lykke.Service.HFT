using AssetsCache;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
        Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId);
        Task<ResponseModel> CancelAllAsync(string clientId, AssetPair pair, bool? isBuy);
        Task<ResponseModel<MarketOrderResponseModel>> PlaceMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, decimal volume, bool straight, double? reservedLimitVolume = null);
        Task<ResponseModel<BulkOrderResponseModel>> PlaceBulkLimitOrderAsync(string clientId, AssetPair assetPair, IEnumerable<BulkOrderItemModel> items, bool cancelPrevious = false, CancelMode? cancelMode = default);
        Task<ResponseModel<LimitOrderResponseModel>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, decimal volume, decimal price, bool cancelPreviousOrders = false);
        Task<ResponseModel<LimitOrderResponseModel>> PlaceStopLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume, double? lowerPrice, double? lowerLimitPrice, double? upperPrice, double? upperLimitPrice, bool cancelPreviousOrders = false);
    }
}
