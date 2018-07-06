using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Validators;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Request model for placing new limit orders.
    /// </summary>
    [PublicAPI]
    public class PlaceLimitOrderModel
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
        /// The price for which you want to buy or sell the asset.
        /// </summary>
        [Required]
        [DoubleGreaterThanZero]
        public double Price { get; set; }
    }
}
