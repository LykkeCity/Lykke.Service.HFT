using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client;
using Lykke.Service.HFT.Core.Services.Assets;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.HFT.Services.Assets
{
    public class AssetPairsManager : IAssetPairsManager
    {
        private readonly IAssetsServiceWithCache _apiService;

        public AssetPairsManager(IAssetsServiceWithCache apiService)
        {
            _apiService = apiService;
        }

        public async Task<AssetPair> TryGetEnabledAssetPairAsync(string assetPairId)
        {
            var pair = await _apiService.TryGetAssetPairAsync(assetPairId);

            return pair == null || pair.IsDisabled ? null : pair;
        }

        public async Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync()
        {
            return (await _apiService.GetAllAssetPairsAsync()).Where(a => !a.IsDisabled);
        }


        public async Task<Asset> TryGetEnabledAssetAsync(string assetId)
        {
            var asset = await _apiService.TryGetAssetAsync(assetId);

            return asset == null || asset.IsDisabled ? null : asset;
        }

        public async Task<IEnumerable<Asset>> GetAllEnabledAssetsAsync()
        {
            return (await _apiService.GetAllAssetsAsync()).Where(a => !a.IsDisabled);
        }
    }
}
