using System;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Response model for placing new limit orders.
    /// </summary>
    [PublicAPI]
    public class LimitOrderResponseModel
    {
        /// <summary>
        /// The identifier under which the limit order was placed.
        /// </summary>
        public Guid Id { get; set; }
    }
}