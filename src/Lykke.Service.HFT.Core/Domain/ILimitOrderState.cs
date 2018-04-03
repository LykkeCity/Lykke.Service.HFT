using System;

namespace Lykke.Service.HFT.Core.Domain
{
    public interface ILimitOrderState
    {
        string AssetPairId { get; }
        string ClientId { get; }
        DateTime CreatedAt { get; }
        Guid Id { get; }
        DateTime? LastMatchTime { get; }
        double? Price { get; }
        DateTime Registered { get; }
        double RemainingVolume { get; }
        OrderStatus Status { get; }
        double Volume { get; }
    }
}