using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.OrderBook
{
    /// <summary>
    /// The volume and price of an order in the orderbook.
    /// </summary>
    [PublicAPI]
    public class VolumePriceModel
    {
        /// <summary>
        /// The volume of the order.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// The price of the order.
        /// </summary>
        public double Price { get; set; }
    }
}