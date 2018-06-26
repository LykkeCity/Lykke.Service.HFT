using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Wallets
{
    /// <summary>
    /// Balance response model for the wallets balance request.
    /// </summary>
    [PublicAPI]
    public class BalanceModel
    {
        /// <summary>
        /// The asset identifier.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The asset balance.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// The reserved asset balance.
        /// </summary>
        public decimal Reserved { get; set; }
    }
}