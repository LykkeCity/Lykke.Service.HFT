using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Response model for placing new bulk orders.
    /// </summary>
    [PublicAPI]
    public class BulkOrderResponseModel
    {
        /// <summary>
        /// The asset pair of this bulk order.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The possible error that occured when processing this bulk order. Check individual statusses for the status per order.
        /// </summary>
        public ErrorCodeType? Error { get; set; }

        /// <summary>
        /// The bulk order item statuses.
        /// </summary>
        public IReadOnlyList<BulkOrderItemStatusModel> Statuses { get; set; }
    }
}