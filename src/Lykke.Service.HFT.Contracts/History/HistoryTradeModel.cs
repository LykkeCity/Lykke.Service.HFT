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
        public Guid Id { get; set; }

        /// <summary>
        /// Related order Id.
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// The time the trade.
        /// </summary>
        [Obsolete("Use Timestamp instead")]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The time the trade.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The current status of the trade.
        /// </summary>
        [Obsolete("Deprecated")]
        public string State { get; set; }

        /// <summary>
        /// The trade amount.
        /// </summary>
        [Obsolete("Use BaseVolume instead")]
        public decimal Amount { get; set; }

        public decimal BaseVolume { get; set; }
        public decimal QuotingVolume { get; set; }

        /// <summary>
        /// The trade base asset.
        /// </summary>
        [Obsolete("Use BaseAssetId instead")]
        public string Asset { get; set; }

        /// <summary>
        /// The trade base asset.
        /// </summary>
        public string BaseAssetId { get; set; }

        /// <summary>
        /// The trade quoting asset.
        /// </summary>
        public string QuotingAssetId { get; set; }

        /// <summary>
        /// The trading pair.
        /// </summary>
        [Obsolete("Use AssetPairId instead")]
        public string AssetPair { get; set; }

        /// <summary>
        /// The trading pair.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The trading price.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// The applied trading fee.
        /// </summary>
        public FeeModel Fee { get; set; }
    }
}