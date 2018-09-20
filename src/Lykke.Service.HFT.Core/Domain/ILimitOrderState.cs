using System;
using Lykke.Service.HFT.Contracts.Orders;

namespace Lykke.Service.HFT.Core.Domain
{
    public interface ILimitOrderState
    {
        string AssetPairId { get; set; }
        string ClientId { get; set; }
        DateTime CreatedAt { get; set; }
        Guid Id { get; }
        DateTime? LastMatchTime { get; set; }
        double? Price { get; set; }
        DateTime Registered { get; set; }
        double RemainingVolume { get; set; }
        OrderStatus Status { get; set; }
        double Volume { get; set; }
        int Type { get; set; }
        double? LowerLimitPrice { get; set; }
        double? LowerPrice { get; set; }
        double? UpperLimitPrice { get; set; }
        double? UpperPrice { get; set; }
    }
}