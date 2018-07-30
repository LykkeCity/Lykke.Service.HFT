using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Order status enumeration for the orders to query.
    /// </summary>
    [PublicAPI]
    public enum OrderStatusQuery
    {
        /// <summary>
        /// All orders
        /// </summary>
        All,
        /// <summary>
        /// Open orders: InOrderBook, Processing and Pending
        /// </summary>
        Open,
        /// <summary>
        /// Limit order in order book
        /// </summary>
        InOrderBook,
        /// <summary>
        /// Partially matched
        /// </summary>
        Processing,
        /// <summary>
        /// Fully matched
        /// </summary>
        Matched,
        /// <summary>
        /// Canceled
        /// </summary>
        Cancelled,
        /// <summary>
        /// Rejected
        /// </summary>
        Rejected,
        /// <summary>
        /// Replaced orders
        /// </summary>
        Replaced
    }
}
