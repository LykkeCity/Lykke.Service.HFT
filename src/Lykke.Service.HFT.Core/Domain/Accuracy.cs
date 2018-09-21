using Common;
using Lykke.Service.Assets.Client.Models.v3;

namespace Lykke.Service.HFT.Core.Domain
{
    public static class Accuracy
    {
        public static decimal TruncatePrice(this decimal price, AssetPair assetPair, bool inverted = false)
            => price.TruncateDecimalPlaces(inverted ? assetPair.InvertedAccuracy : assetPair.Accuracy);

        public static decimal? TruncatePrice(this decimal? price, AssetPair assetPair, bool inverted = false)
            => price?.TruncateDecimalPlaces(inverted ? assetPair.InvertedAccuracy : assetPair.Accuracy);

        public static decimal TruncateVolume(this decimal price, Asset asset)
            => price.TruncateDecimalPlaces(asset.Accuracy);
    }
}