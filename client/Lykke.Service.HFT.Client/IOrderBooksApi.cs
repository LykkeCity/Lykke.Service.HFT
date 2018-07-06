using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.OrderBook;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service inteface to query the Lykke exchange order books.
    /// </summary>
    [PublicAPI]
    public interface IOrderBooksApi
    {
        /// <summary>
        /// Gets the orderbook of the specified asset pair ID.
        /// </summary>
        /// <param name="assetPairId">The asset pair to query.</param>
        [Get("/api/OrderBooks/{assetPairId}")]
        Task<IEnumerable<OrderBookModel>> Get(string assetPairId);

        /// <summary>
        /// Gets all orderbooks for all asset pairs.
        /// </summary>
        [Get("/api/OrderBooks")]
        Task<IEnumerable<OrderBookModel>> GetAll();
    }
}