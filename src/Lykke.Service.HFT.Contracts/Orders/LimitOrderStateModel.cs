﻿using System;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Response model class for limit order state.
    /// </summary>
    [PublicAPI]
    public class LimitOrderStateModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The limit order status.
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// The asset pair identifier.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The volume of the limit order.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// The price of the limit order.
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// The remaining volume of the order.
        /// </summary>
        public double RemainingVolume { get; set; }

        /// <summary>
        /// The last match date time.
        /// </summary>
        public DateTime? LastMatchTime { get; set; }

        /// <summary>
        /// The created at date time.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The type of limit order.
        /// </summary>
        public LimitOrderType Type { get; set; }

        /// <summary>
        /// The lower limit price of the limit order.
        /// </summary>
        public double? LowerLimitPrice { get; set; }

        /// <summary>
        /// The lower price of the limit order.
        /// </summary>
        public double? LowerPrice { get; set; }

        /// <summary>
        /// The upper limit price of the limit order.
        /// </summary>
        public double? UpperLimitPrice { get; set; }

        /// <summary>
        /// The upper price of the limit order.
        /// </summary>
        public double? UpperPrice { get; set; }
    }
}
