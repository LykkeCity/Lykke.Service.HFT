using System;

namespace Lykke.Service.HFT.Contracts.History
{
    /// <summary>
    /// Response model for quering placed trades.
    /// </summary>
    public class HistoryTradeModel
    {
        /// <summary>
        /// The ID of the trade.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The time the trade was placed.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The current status of the trade.
        /// </summary>
        public TradeStatus State { get; set; }

        /// <summary>
        /// The trade amount.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The trade base asset.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The trading pair.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// The trading price.
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// The applied trading fee.
        /// </summary>
        public FeeModel Fee { get; set; }
    }
}