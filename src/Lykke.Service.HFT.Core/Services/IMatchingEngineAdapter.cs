using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Contracts.Orders;
using OrderAction = Lykke.Service.HFT.Contracts.Orders.OrderAction;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
        Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId);
        Task<ResponseModel> CancelAllAsync(string clientId, AssetPair pair, bool? isBuy);
        Task<ResponseModel<MarketOrderResponseModel>> PlaceMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume, bool straight, double? reservedLimitVolume = default);
        Task<ResponseModel<LimitOrderResponseModel>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume, double price, bool cancelPreviousOrders = false);
        Task<ResponseModel<BulkOrderResponseModel>> PlaceBulkLimitOrderAsync(string clientId, AssetPair assetPair, IEnumerable<BulkOrderItemModel> items, bool cancelPrevious = false, CancelMode? cancelMode = default);
    }
}
