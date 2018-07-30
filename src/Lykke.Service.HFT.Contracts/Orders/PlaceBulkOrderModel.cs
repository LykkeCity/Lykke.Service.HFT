using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Request model for placing bulk limit orders.
    /// </summary>
    [PublicAPI]
    public class PlaceBulkOrderModel
    {
        /// <summary>
        /// The asset pair you want to buy or sell.
        /// </summary>
        [Required]
        public string AssetPairId { get; set; }

        /// <summary>
        /// Cancel the previous orders of this asset pair.
        /// </summary>
        public bool CancelPreviousOrders { get; set; }

        /// <summary>
        /// [Optional] The cancel mode behavior for cancelling previous orders.
        /// </summary>
        public CancelMode? CancelMode { get; set; }

        /// <summary>
        /// The orders you want to place.
        /// </summary>
        public IEnumerable<BulkOrderItemModel> Orders { get; set; }
    }
}