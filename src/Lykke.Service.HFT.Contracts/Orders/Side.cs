using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Side enumeration describing the type of orders to query.
    /// </summary>
    [PublicAPI]
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