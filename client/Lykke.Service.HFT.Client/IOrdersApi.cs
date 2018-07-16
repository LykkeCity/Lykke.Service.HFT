using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Orders;
using Refit;

namespace Lykke.Service.HFT.Client
{
    /// <summary>
    /// Service inteface for high-frequency trading orders functionality.
    /// </summary>
    /// <remarks>
    /// Service expects api-key header for authentification. This can be send using a custom handler or the Lykke WithApiKey call on the client generator.
    /// </remarks>
    [PublicAPI]
    [Headers("api-key")]
    public interface IOrdersApi
    {
        /// <summary>
        /// Gets the state of a specific order.
        /// </summary>
        /// <param name="id">The order identifier.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        /// <returns>the found order</returns>
        [Get("/api/Orders/{id}")]
        Task<LimitOrderStateModel> GetOrder(Guid id, [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Queries the orders based on status.
        /// </summary>
        /// <param name="status">[optional] The status to query, default 'All'.</param>
        /// <param name="take">[optional] The amount of orders to take, default 100 and max 500.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        /// <returns>the found orders</returns>
        [Get("/api/Orders")]
        Task<IReadOnlyCollection<LimitOrderStateModel>> GetOrders(
            [Query] OrderStatusQuery status = OrderStatusQuery.All,
            [Query] int? take = 100,
            [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Places a new market order.
        /// </summary>
        /// <param name="marketOrder">The market order data.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        /// <returns>the average settled price</returns>
        [Post("/api/Orders/v2/market")]
        Task<MarketOrderResponseModel> PlaceMarketOrder(
            [Body] PlaceMarketOrderModel marketOrder,
            [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Places a new limit order.
        /// </summary>
        /// <param name="limitOrder">The limit order data.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        /// <returns>the limit order ID</returns>
        [Post("/api/Orders/v2/limit")]
        Task<LimitOrderResponseModel> PlaceLimitOrder(
            [Body] PlaceLimitOrderModel limitOrder,
            [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Cancels the specific limit order.
        /// </summary>
        /// <param name="id">The order identifier.</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Delete("/api/Orders/{id}")]
        Task CancelLimitOrder(Guid id, [Header("api-key")] string apiKey = null);

        /// <summary>
        /// Cancels all open limit orders.
        /// </summary>
        /// <param name="assetPairId">[optional] id of asset-pair orders to cancel</param>
        /// <param name="side">[optional] the type of orders to cancel (Buy, Sell or Both)</param>
        /// <param name="apiKey">The API key header. Can also be send using a custom handler or the Lykke WithApiKey call on the client generator.</param>
        [Delete("/api/Orders")]
        Task CancelAll(
            [Query] string assetPairId = null,
            [Query] Side? side = null,
            [Header("api-key")] string apiKey = null);
    }
}