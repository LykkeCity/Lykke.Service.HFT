using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
        Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId);
        Task<ResponseModel> CancelAllAsync(string clientId, AssetPair pair, bool? isBuy);
        Task<ResponseModel<double>> HandleMarketOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume, bool straight, double? reservedLimitVolume = default(double?));
        Task<ResponseModel<Guid>> PlaceLimitOrderAsync(string clientId, AssetPair assetPair, OrderAction orderAction, double volume, double price, bool cancelPreviousOrders = false);

    }
}
