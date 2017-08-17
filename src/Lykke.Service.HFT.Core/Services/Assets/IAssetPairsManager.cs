using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Service.HFT.Core.Services.Assets
{
	public interface IAssetPairsManager
	{
		Task<IAssetPair> TryGetEnabledPairAsync(string assetPairId);
		Task<IEnumerable<IAssetPair>> GetAllEnabledAsync();
	}
}
