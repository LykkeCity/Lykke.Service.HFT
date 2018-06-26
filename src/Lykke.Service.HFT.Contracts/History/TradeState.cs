using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.History
{
    /// <summary>
    /// Trade status enumeration
    /// </summary>
    [PublicAPI]
    public enum TradeStatus
    {
        /// <summary>
        /// Unknown trading state
        /// </summary>
        Unknown,

        /// <summary>
        /// Trade is in progess
        /// </summary>
        InProgress,

        /// <summary>
        /// Trade has finished
        /// </summary>
        Finished,

        /// <summary>
        /// Trade is cancelled
        /// </summary>
        Canceled,

        /// <summary>
        /// Trade has failed
        /// </summary>
        Failed,
    }
}