using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Order model for bulk limit orders.
    /// </summary>
    [PublicAPI]
    public class BulkOrderItemModel
    {
        /// <summary>
        /// The order action, buy or sell.
        /// </summary>
        [Required]
        public OrderAction OrderAction { get; set; }

        /// <summary>
        /// The volume of the order.
        /// </summary>
        [Required]
        public double Volume { get; set; }

        /// <summary>
        /// The price of the order.
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// [Optional] The old order identifier that needs to be replaced with this order.
        /// </summary>
        public string OldId { get; set; }
    }
}