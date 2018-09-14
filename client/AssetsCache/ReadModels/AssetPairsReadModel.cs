using Autofac;
using Lykke.Service.Assets.Client;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace AssetsCache.ReadModels
{
    class AssetPairsReadModel : IAssetPairsReadModel, IStartable
    {
        private readonly IAssetsService _assetsService;
        private readonly IMemoryCache _cache;

        public AssetPairsReadModel(IAssetsService assetsService, IMemoryCache cache)
        {
            _assetsService = assetsService;
            _cache = cache;
        }

        public AssetPair Get(string id)
        {
            try
            {
                if (!_cache.TryGetValue(id, out AssetPair value))
                    return null;
                return value;
            }
            catch (System.InvalidCastException)
            {
                return null;
            }
        }

        public IReadOnlyCollection<AssetPair> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            var assetPairs = _assetsService.AssetPairGetAll();
            foreach (var assetPair in assetPairs)
            {
                _cache.Set(assetPair.Id, Mapper.ToAssetPair(assetPair));
            }
        }
    }
}