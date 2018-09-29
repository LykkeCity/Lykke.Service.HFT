using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// The same as <see cref="Lykke.Service.History.Contracts.Enums.OrderType"/> 
    /// </summary>
    [PublicAPI]
    public enum OrderType
    {
        Unknown,
        Market,
        Limit,
        StopLimit,
    }
}