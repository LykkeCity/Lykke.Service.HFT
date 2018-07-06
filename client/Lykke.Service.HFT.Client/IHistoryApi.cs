using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.History;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service inteface for high-frequency trading history functionality.
    /// </summary>
    /// <remarks>
    /// Service expects api-key header for authentification. This can be send using a custom handler or the Lykke WithApiKey call on the client generator.
    /// </remarks>
    [PublicAPI]
    [Headers("api-key")]
    public interface IHistoryApi
    {
        /// <summary>
        /// Gets the a specifc trade.
        /// </summary>
        /// <param name="tradeId">The trade identifier.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Get("/api/History/trades/{tradeId}")]
        Task<HistoryTradeModel> GetTrade(string tradeId, [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Gets the latest trades by asset identifier of an api wallet.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Get("/api/History/trades")]
        Task<IReadOnlyCollection<HistoryTradeModel>> GetLatestTradesByAssetId([Query] string assetId, [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Gets the latest trades by asset and asset pair identifier of an api wallet.
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <param name="assetPairId">The asset pair identifier.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Get("/api/History/trades")]
        Task<IReadOnlyCollection<HistoryTradeModel>> GetLatestTradesByAssetPairId([Query] string assetId, [Query] string assetPairId, [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Advanced query to get the latest trades of an api wallet.
        /// </summary>
        /// <param name="query">The query parameters.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Get("/api/History/trades")]
        Task<IReadOnlyCollection<HistoryTradeModel>> GetLatestTrades(LatestTradesQueryModel query, [Header("api-key")] string apiKey = null);
    }
}