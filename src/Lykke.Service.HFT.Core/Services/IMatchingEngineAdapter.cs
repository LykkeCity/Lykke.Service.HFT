using Lykke.Service.Assets.Client.Models.v3;
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
        Task<ResponseModel> CancelAllAsync(string walletId, AssetPair pair, bool? isBuy);
        Task<ResponseModel<MarketOrderResponseModel>> PlaceMarketOrderAsync(string walletId, AssetPair assetPair, OrderAction orderAction, decimal volume, bool straight, double? reservedLimitVolume = null);
        Task<ResponseModel<BulkOrderResponseModel>> PlaceBulkLimitOrderAsync(string walletId, AssetPair assetPair, IEnumerable<BulkOrderItemModel> items, bool cancelPrevious = false, CancelMode? cancelMode = default);
        Task<ResponseModel<LimitOrderResponseModel>> PlaceLimitOrderAsync(string walletId, AssetPair assetPair, OrderAction orderAction, decimal volume, decimal price, bool cancelPreviousOrders = false);
        Task<ResponseModel<LimitOrderResponseModel>> PlaceStopLimitOrderAsync(string walletId, AssetPair assetPair, OrderAction orderAction, decimal volume, decimal? lowerPrice, decimal? lowerLimitPrice, decimal? upperPrice, decimal? upperLimitPrice, bool cancelPreviousOrders = false);
    }
}
