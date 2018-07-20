using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Limit order type enumeration
    /// </summary>
    [PublicAPI]
    public enum LimitOrderType
    {
        /// <summary>
        /// Default limit order
        /// </summary>
        Default = 0,

        /// <summary>
        /// Stop limit order
        /// </summary>
        Stop = 1
    }
}