namespace AssetsCache.ReadModels
{
    public static class AssetsReadModelExtensions
    {
        public static Asset GetIfEnabled(this IAssetsReadModel readModel, string id)
        {
            var asset = readModel.Get(id);
            return asset != null && !asset.IsDisabled ? asset : null;
        }

        public static AssetPair GetIfEnabled(this IAssetPairsReadModel readModel, string id)
        {
            var assetPair = readModel.Get(id);
            return assetPair != null && !assetPair.IsDisabled ? assetPair : null;
        }
    }
}
