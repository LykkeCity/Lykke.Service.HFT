using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Service.HFT.Core.Services
{
	public interface IAssetPairsManager
	{
		Task<IAssetPair> TryGetEnabledPairAsync(string assetPairId);
		Task<IEnumerable<IAssetPair>> GetAllEnabledAsync();
	}
}
