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
        public double Amount { get; set; }

        /// <summary>
        /// The fee type.
        /// </summary>
        public FeeType Type { get; set; }
    }
}