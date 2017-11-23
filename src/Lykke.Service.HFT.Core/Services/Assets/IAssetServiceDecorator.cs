using Lykke.Service.Assets.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.Assets
{
    public interface IAssetServiceDecorator
    {
        Task<AssetPair> GetEnabledAssetPairAsync(string assetPairId);
        Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync();
        Task<Asset> GetEnabledAssetAsync(string assetPairId);
        Task<IEnumerable<Asset>> GetAllEnabledAssetsAsync();
    }
}
