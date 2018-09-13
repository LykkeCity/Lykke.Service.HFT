using Lykke.Service.Assets.Client;
using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Lykke.Service.HFT.Services
{
    public class AssetServiceDecorator : IAssetServiceDecorator
    {
        private readonly IAssetsService _apiService;
        private readonly IMemoryCache _cache;

        public AssetServiceDecorator(IAssetsService apiService, IMemoryCache cache)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _cache = cache;
        }

        public async Task<AssetPair> GetEnabledAssetPairAsync(string assetPairId)
        {
            var pair = _cache.Get<AssetPair>(assetPairId);
            return pair == null || pair.IsDisabled ? null : pair;
        }

        public async Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync()
        {
            //return AutoMapper.Mapper.Map<List<AssetPair>>((await _apiService.GetAllAssetPairsAsync()).Where(a => !a.IsDisabled));
            return null;
        }

        public async Task<Asset> GetAssetAsync(string assetId)
            => _cache.Get<Asset>(assetId);


        public async Task<Asset> GetEnabledAssetAsync(string assetId)
        {
            var asset = _cache.Get<Asset>(assetId);
            return asset == null || asset.IsDisabled ? null : asset;
        }
    }
}
