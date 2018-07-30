using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Validators;

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
        [DoubleGreaterThanZero]
        public double Volume { get; set; }

        /// <summary>
        /// The price of the order.
        /// </summary>
        [Required]
        [DoubleGreaterThanZero]
        public double Price { get; set; }

        /// <summary>
        /// [Optional] The old order identifier that needs to be replaced with this order.
        /// </summary>
        /// <remarks>Old order will get status replaced.</remarks> 
        public string OldId { get; set; }
    }
}