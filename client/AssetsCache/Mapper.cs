using Lykke.Service.Assets.Contract.Events;

namespace AssetsCache
{
    /// <summary>
    /// This manual mapping is used instead of AutoMapper to reduce complexity of Assets client code usage.
    /// </summary>
    static class Mapper
    {
        public static Asset ToAsset(AssetCreatedEvent evt)
        {
            return new Asset
            {
                Id = evt.Id,
                IsDisabled = evt.IsDisabled,
                DisplayId = evt.DisplayId,
                Accuracy = evt.Accuracy
            };
        }

        public static Asset ToAsset(AssetUpdatedEvent evt)
        {
            return new Asset
            {
                Id = evt.Id,
                IsDisabled = evt.IsDisabled,
                DisplayId = evt.DisplayId,
                Accuracy = evt.Accuracy
            };
        }

        public static Asset ToAsset(Lykke.Service.Assets.Client.Models.Asset asset)
        {
            return new Asset
            {
                Id = asset.Id,
                IsDisabled = asset.IsDisabled,
                DisplayId = asset.DisplayId,
                Accuracy = asset.Accuracy
            };
        }

        public static AssetPair ToAssetPair(AssetPairCreatedEvent evt)
        {
            return new AssetPair
            {
                Id = evt.Id,
                IsDisabled = evt.IsDisabled,
                Name = evt.Name,
                Accuracy = evt.Accuracy,
                InvertedAccuracy = evt.Accuracy,
                BaseAssetId = evt.BaseAssetId,
                QuotingAssetId = evt.QuotingAssetId,
                MinVolume = evt.MinVolume,
                MinInvertedVolume = evt.MinInvertedVolume
            };
        }

        public static AssetPair ToAssetPair(AssetPairUpdatedEvent evt)
        {
            return new AssetPair
            {
                Id = evt.Id,
                IsDisabled = evt.IsDisabled,
                Name = evt.Name,
                Accuracy = evt.Accuracy,
                InvertedAccuracy = evt.Accuracy,
                BaseAssetId = evt.BaseAssetId,
                QuotingAssetId = evt.QuotingAssetId,
                MinVolume = evt.MinVolume,
                MinInvertedVolume = evt.MinInvertedVolume
            };
        }

        public static AssetPair ToAssetPair(Lykke.Service.Assets.Client.Models.AssetPair assetPair)
        {
            return new AssetPair
            {
                Id = assetPair.Id,
                IsDisabled = assetPair.IsDisabled,
                Name = assetPair.Name,
                Accuracy = assetPair.Accuracy,
                InvertedAccuracy = assetPair.Accuracy,
                BaseAssetId = assetPair.BaseAssetId,
                QuotingAssetId = assetPair.QuotingAssetId,
                MinVolume = (decimal)assetPair.MinVolume,
                MinInvertedVolume = (decimal)assetPair.MinInvertedVolume
            };
        }
    }
}
