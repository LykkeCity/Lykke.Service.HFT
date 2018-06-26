using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.History
{
    /// <summary>
    /// Fee type enumeration
    /// </summary>
    [PublicAPI]
    public enum FeeType
    {
        /// <summary>
        /// Unknown fee
        /// </summary>
        Unknown,

        /// <summary>
        /// Absolute fee
        /// </summary>
        Absolute,

        /// <summary>
        /// Relative fee (percentage)
        /// </summary>
        Relative,
    }
}