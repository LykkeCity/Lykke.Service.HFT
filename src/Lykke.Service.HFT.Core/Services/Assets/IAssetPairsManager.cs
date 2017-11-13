using Lykke.Service.Assets.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.HFT.Core.Services.Assets
{
	public interface IAssetPairsManager
	{
		Task<AssetPair> TryGetEnabledAssetPairAsync(string assetPairId);
		Task<IEnumerable<AssetPair>> GetAllEnabledAssetPairsAsync();
	    Task<Asset> TryGetEnabledAssetAsync(string assetPairId);
	    Task<IEnumerable<Asset>> GetAllEnabledAssetsAsync();
    }
}
