using Common;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.HFT.Core.Domain
{
    public static class Accuracy
    {
        public static double TruncatePrice(this double price, AssetPair assetPair, bool inverted = false)
            => price.TruncateDecimalPlaces(inverted ? assetPair.InvertedAccuracy : assetPair.Accuracy);

        public static double? TruncatePrice(this double? price, AssetPair assetPair, bool inverted = false)
            => price?.TruncateDecimalPlaces(inverted ? assetPair.InvertedAccuracy : assetPair.Accuracy);

        public static double TruncateVolume(this double price, Asset asset)
            => price.TruncateDecimalPlaces(asset.Accuracy);
    }
}