using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.HFT.Contracts.Validators;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Request model for placing new market orders.
    /// </summary>
    [PublicAPI]
    public class PlaceMarketOrderModel
    {
        /// <summary>
        /// The asset pair identifier, eg BTCUSD.
        /// </summary>
        [Required]
        public string AssetPairId { get; set; }

        /// <summary>
        /// The asset that you want to buy or sell, eg BTC in case of BTCUSD.
        /// </summary>
        [Required]
        public string Asset { get; set; }

        /// <summary>
        /// The type of order (Buy or Sell).
        /// </summary>
        [Required]
        public OrderAction OrderAction { get; set; }

        /// <summary>
        /// The amount of the asset you want to buy or sell, should be at least the minimum volume of the asset pair.
        /// </summary>
        [Required]
        [DecimalGreaterThanZero]
        public decimal Volume { get; set; }
    }
}
