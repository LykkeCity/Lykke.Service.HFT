using Lykke.Service.Assets.Client.Models;

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
    }
}
