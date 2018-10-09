using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Status of the limit order.
    /// The same as <see cref="Lykke.Service.History.Contracts.Enums.OrderStatus"/>
    /// </summary>
    [PublicAPI]
    public enum OrderStatus
    {
        Unknown,
        Placed,
        PartiallyMatched,
        Matched,
        Pending,
        Cancelled,
        Replaced,
        Rejected
    }
}