using System;
using Lykke.Service.HFT.Contracts.Orders;

namespace Lykke.Service.HFT.Contracts.Events
{
    /// <summary>
    /// Limit order update event order contract class.
    /// </summary>
    /// <remarks>TODO check if this class can be merged with Orders.LimitOrderStateModel</remarks>
    public class Order
    {
        /// <summary>
        /// The order ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The order status.
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// The order asset-pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The order volume.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// The order price.
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// The remaining order volume.
        /// </summary>
        public double RemainingVolume { get; set; }

        /// <summary>
        /// The time the order was last matched.
        /// </summary>
        public DateTime? LastMatchTime { get; set; }
    }
}