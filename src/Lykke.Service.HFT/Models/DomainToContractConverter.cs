using Lykke.Service.Assets.Client.Models;
using Lykke.Service.OperationsRepository.Contract.Cash;

namespace Lykke.Service.HFT.Models
{
    public static class DomainToContractConverter
    {
        public static AssetPairModel ConvertToApiModel(this AssetPair src)
        {
            if (src == null)
                return null;

            return new AssetPairModel
            {
                Id = src.Id,
                Name = src.Name,
                Accuracy = src.Accuracy,
                InvertedAccuracy = src.InvertedAccuracy,
                BaseAssetId = src.BaseAssetId,
                QuotingAssetId = src.QuotingAssetId
            };
        }

        public static HistoryTradeModel ConvertToApiModel(this ClientTradeDto src)
        {
            return new HistoryTradeModel
            {
                AssetId = src.AssetId,
                DateTime = src.DateTime,
                Id = src.Id,
                LimitOrderId = src.LimitOrderId,
                MarketOrderId = src.MarketOrderId,
                Price = src.Price,
                Amount = src.Amount,
                State = src.State.ToString()
            };
        }

        public static LimitOrderState ConvertToApiModel(this Core.Domain.LimitOrderState order)
        {
            return new LimitOrderState
            {
                Id = order.Id,
                AssetPairId = order.AssetPairId,
                CreatedAt = order.CreatedAt,
                LastMatchTime = order.LastMatchTime,
                Price = order.Price,
                RemainingVolume = order.RemainingVolume,
                Status = order.Status.ToString(),
                Volume = order.Volume
            };
        }
    }
}
