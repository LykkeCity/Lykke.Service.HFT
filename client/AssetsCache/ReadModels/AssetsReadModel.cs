using Autofac;
using Lykke.Service.Assets.Client;
using Microsoft.Extensions.Caching.Memory;

namespace AssetsCache.ReadModels
{
    class AssetsReadModel : IAssetsReadModel, IStartable
    {
        private readonly IAssetsService _assetsService;
        private readonly IMemoryCache _cache;

        public AssetsReadModel(IAssetsService assetsService, IMemoryCache cache)
        {
            _assetsService = assetsService;
            _cache = cache;
        }

        public Asset Get(string id)
        {
            try
            {
                if (!_cache.TryGetValue(id, out Asset value))
                    return null;
                return value;
            }
            catch (System.InvalidCastException)
            {
                return null;
            }
        }
        
        public void Start()
        {
            var assets = _assetsService.AssetGetAll(true);
            foreach (var asset in assets)
            {
                _cache.Set(asset.Id, AutoMapper.Mapper.Map<Asset>(asset));
            }
        }
    }
}