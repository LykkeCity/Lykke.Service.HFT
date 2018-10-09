using System;

namespace Lykke.Service.HFT.Wamp.Events
{
    /// <summary>
    /// Limit order update event trade contract class.
    /// </summary>
    /// <remarks>TODO check if this class can be merged with History.HistoryTradeModel</remarks>
    public class Trade
    {
        /// <summary>
        /// The traded asset.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The traded volume.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// The trade price.
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// The traded timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The opposite asset of the trade.
        /// </summary>
        public string OppositeAsset { get; set; }

        /// <summary>
        /// The opposite asset volume of the trade.
        /// </summary>
        public double OppositeVolume { get; set; }
    }
}