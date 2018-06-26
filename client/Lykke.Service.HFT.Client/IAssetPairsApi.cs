using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Assets;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service inteface to query high-frequency trading asset pairs.
    /// </summary>
    [PublicAPI]
    public interface IAssetPairsApi
    {
        /// <summary>
        /// Gets a specific asset pair by id.
        /// </summary>
        /// <param name="assetPairId">The asset pair id, eg BTCUSD.</param>
        [Get("/api/AssetPairs/{assetPairId}")]
        Task<AssetPairModel> Get(string assetPairId);

        /// <summary>
        /// Gets all enabled asset pairs.
        /// </summary>
        [Get("/api/AssetPairs")]
        Task<IReadOnlyCollection<AssetPairModel>> GetAll();
    }
}