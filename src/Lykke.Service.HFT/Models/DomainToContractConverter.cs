using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core.Domain;
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
            return new HistoryTradeModel
            {
                Id = src.Id,
                DateTime = src.DateTime,
                State = src.State,
                Amount = src.Amount,
                Asset = src.Asset,
                AssetPair = src.AssetPair,
                Price = src.Price,
                Fee = new Fee
                {
                    Amount = src.FeeSize,
                    Type = src.FeeType
                }
            };
        }

        public static LimitOrderState ConvertToApiModel(this ILimitOrderState order)
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
