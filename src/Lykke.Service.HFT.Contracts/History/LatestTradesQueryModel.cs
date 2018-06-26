using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.History
{
    /// <summary>
    /// Historic trades query model
    /// </summary>
    [PublicAPI]
    public class LatestTradesQueryModel
    {
        /// <summary>
        /// The trade asset ID.
        /// </summary>
        [Required]
        public string AssetId { get; set; }

        /// <summary>
        /// The asset pair ID.
        /// </summary>
        /// <remarks>optional</remarks>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The amount of trades to skip.
        /// </summary>
        /// <remarks>optional, default 0</remarks>
        public int? Skip { get; set; } = 0;

        /// <summary>
        /// The amount of trades to take.
        /// </summary>
        /// <remarks>optional, default 100, maximum 500</remarks>
        public int? Take { get; set; } = 100;
    }
}