namespace Lykke.Service.HFT.Models.Requests
{
    /// <summary>
    /// Side enumeration describing the type of orders to query.
    /// </summary>
    public enum Side
    {
        /// <summary>
        /// Both types of orders (Buy and Sell)
        /// </summary>
        Both,

        /// <summary>
        /// Buy orders
        /// </summary>
        Buy,

        /// <summary>
        /// Sell orders
        /// </summary>
        Sell
    }
}