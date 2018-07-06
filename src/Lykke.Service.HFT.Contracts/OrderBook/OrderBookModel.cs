using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.OrderBook
{
    /// <summary>
    /// Response model for orderbook queries.
    /// </summary>
    [PublicAPI]
    public class OrderBookModel
    {
        /// <summary>
        /// The asset pair of the order.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Is the order a buy order, otherwise false in case of a sell order.
        /// </summary>
        public bool IsBuy { get; set; }

        /// <summary>
        /// The timestamp of the placed order.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The prices and volumes of the order.
        /// </summary>
        public IReadOnlyCollection<VolumePriceModel> Prices { get; set; }
    }
}
