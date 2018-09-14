using System.Collections.Generic;

namespace AssetsCache.ReadModels
{
    public interface IAssetPairsReadModel
    {
        AssetPair Get(string id);
        IReadOnlyCollection<AssetPair> GetAll();
    }
}
