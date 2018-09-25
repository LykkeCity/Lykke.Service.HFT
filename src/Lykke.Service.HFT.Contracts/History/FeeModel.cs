using System;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.History
{
    /// <summary>
    /// Model for a trading fee.
    /// </summary>
    [PublicAPI]
    public class FeeModel
    {
        /// <summary>
        /// The fee amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// The fee type.
        /// </summary>
        [Obsolete("Deprecated")]
        public FeeType Type { get; set; }

        /// <summary>
        /// Asset that was used for fee.
        /// </summary>
        public string FeeAssetId { get; set; }
    }
}