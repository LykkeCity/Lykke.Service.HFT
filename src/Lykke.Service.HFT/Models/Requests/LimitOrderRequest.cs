using Lykke.Service.HFT.Core.Domain;
using Lykke.Service.HFT.Middleware;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Models.Requests
{
    /// <summary>
    /// Request model for a limit order.
    /// </summary>
    public sealed class LimitOrderRequest
    {
        /// <summary>
        /// (Required) The asset pair by id.
        /// </summary>
        [Required]
        public string AssetPairId { get; set; }

        /// <summary>
        /// (Required) The type of limit order (Buy or Sell).
        /// </summary>
        [Required]
        public OrderAction OrderAction { get; set; }

        /// <summary>
        /// (Required) The volume of the limit order. 
        /// Must be positive and the volume cannot be less than the minimum volume as defined for the asset pair.
        /// </summary>
        [Required, DoubleGreaterThanZero]
        public double Volume { get; set; }

        /// <summary>
        /// (Required) The bidding price for the limit order. 
        /// Must be positive.
        /// </summary>
        [Required, DoubleGreaterThanZero]
        public double Price { get; set; }

        /// <summary>
        /// [NotSupportedYet] (Optional) The maximum date &amp; time untill the limit order is cancelled when not fulfilled completely.
        /// Must lie in the future.
        /// </summary>
        [DateTimeInFuture]
        public DateTime? CancelAfter { get; set; }
    }
}
