using System;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Response status model for a specific order in a bulk order.
    /// </summary>
    [PublicAPI]
    public class BulkOrderItemStatusModel
    {
        /// <summary>
        /// The id under which this order was registered. Needed for cancel or status updates.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The possible error that occured when placing this order.
        /// </summary>
        public ErrorCodeType? Error { get; set; }

        /// <summary>
        /// The volume of this order.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The price of this order.
        /// </summary>
        public decimal Price { get; set; }
    }
}