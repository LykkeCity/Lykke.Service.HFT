using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// CancelMode behavior for bulk orders.
    /// </summary>
    [PublicAPI]
    public enum CancelMode
    {
        /// <summary>
        /// Cancel previous orders as defined in the given parameter.
        /// </summary>
        NotEmptySide = 0,

        /// <summary>
        /// Cancel all previous buy- and sell-orders (even if there are no incoming orders)
        /// </summary>
        BothSides = 1,

        /// <summary>
        /// Cancel only previous sell-orders(even if there are no incoming sell-orders)
        /// </summary>
        SellSide = 2,

        /// <summary>
        /// Cancel only previous buy-orders(even if there are no incoming buy-orders)
        /// </summary>
        BuySide = 3,
    }
}