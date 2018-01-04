using Lykke.Service.Assets.Client.Models;
using Lykke.Service.OperationsHistory.AutorestClient.Models;

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
                QuotingAssetId = src.QuotingAssetId,
                MinVolume = src.MinVolume,
                MinInvertedVolume = src.MinInvertedVolume
            };
        }

        public static HistoryTradeModel ConvertToApiModel(this HistoryOperation src)
        {
            if (src.Trade == null) return null;

            return new HistoryTradeModel
            {
                AssetId = src.Trade.Asset,
                DateTime = src.DateTime,
                Id = src.Id,
                LimitOrderId = src.Trade.LimitOrderId,
                MarketOrderId = src.Trade.MarketOrderId,
                Amount = src.Trade.Volume,
                State = src.Trade.State.ToString()
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
