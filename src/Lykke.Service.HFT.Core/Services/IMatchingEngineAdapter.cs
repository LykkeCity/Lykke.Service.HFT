using System;
using System.Threading.Tasks;
using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IMatchingEngineAdapter
    {
        Task<ResponseModel> CancelLimitOrderAsync(Guid limitOrderId);
        Task<ResponseModel<double>> HandleMarketOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, bool straight, double? reservedLimitVolume = default(double?));
        Task<ResponseModel<Guid>> PlaceLimitOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, double price, DateTime? cancelAfter = default(DateTime?), bool cancelPreviousOrders = false);
    }
}
