using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Validators;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Request model for placing new stop limit orders.
    /// </summary>
    [PublicAPI]
    public class PlaceStopLimitOrderModel
    {
        /// <summary>
        /// The asset pair you want to buy or sell.
        /// </summary>
        [Required]
        public string AssetPairId { get; set; }

        /// <summary>
        /// The order action you want to perform (Buy or Sell).
        /// </summary>
        [Required]
        public OrderAction OrderAction { get; set; }

        /// <summary>
        /// The amount of the asset you want to buy or sell, should be at least the minimum volume of the asset pair.
        /// </summary>
        [Required]
        [DoubleGreaterThanZero]
        public double Volume { get; set; }

        /// <summary>
        /// The lower limit price for this stop order.
        /// </summary>
        /// <remarks>
        /// When LowerLimitPrice is send then also LowerPrice is required y vice versa and at least lower or upper prices are required.
        /// </remarks>
        [DoubleGreaterThanZero]
        public double? LowerLimitPrice { get; set; }

        /// <summary>
        /// The lower price for this stop order.
        /// </summary>
        /// <remarks>
        /// When LowerLimitPrice is send then also LowerPrice is required y vice versa and at least lower or upper prices are required.
        /// </remarks>
        [DoubleGreaterThanZero]
        public double? LowerPrice { get; set; }

        /// <summary>
        /// The upper limit price for this stop order.
        /// </summary>
        /// <remarks>
        /// When UpperLimitPrice is send then also UpperPrice is required y vice versa and at least lower or upper prices are required.
        /// </remarks>
        [DoubleGreaterThanZero]
        public double? UpperLimitPrice { get; set; }

        /// <summary>
        /// The upper price for this stop order.
        /// </summary>
        /// <remarks>
        /// When UpperLimitPrice is send then also UpperPrice is required y vice versa and at least lower or upper prices are required.
        /// </remarks>
        [DoubleGreaterThanZero]
        public double? UpperPrice { get; set; }
    }
}