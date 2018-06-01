using Lykke.Service.Assets.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services
{
    public interface IAssetServiceDecorator
    {
        Task<AssetPair> GetEnabledAssetPairAsync(string assetPairId);
        Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync();
        Task<Asset> GetAssetAsync(string assetId);
        Task<Asset> GetEnabledAssetAsync(string assetId);
    }
}
