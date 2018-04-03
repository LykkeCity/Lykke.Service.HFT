namespace Lykke.Service.HFT.Models.Requests
{
    public enum OrderStatus
    {
        /// <summary>
        /// All orders
        /// </summary>
        All,
        /// <summary>
        /// Open orders: InOrderBook, Processing
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
        Rejected
    }
}
