using System;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.HFT.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Services
{
    public class AssetServiceDecorator : IAssetServiceDecorator
    {
        private readonly IAssetsServiceWithCache _apiService;

        public AssetServiceDecorator(IAssetsServiceWithCache apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        public async Task<AssetPair> GetEnabledAssetPairAsync(string assetPairId)
        {
            var pair = await _apiService.TryGetAssetPairAsync(assetPairId);
            return pair == null || pair.IsDisabled ? null : pair;
        }

        public async Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync()
        {
            return (await _apiService.GetAllAssetPairsAsync()).Where(a => !a.IsDisabled);
        }

        public async Task<Asset> GetAssetAsync(string assetId)
            => await _apiService.TryGetAssetAsync(assetId);


        public async Task<Asset> GetEnabledAssetAsync(string assetId)
        {
            var asset = await GetAssetAsync(assetId);
            return asset == null || asset.IsDisabled ? null : asset;
        }
    }
}
