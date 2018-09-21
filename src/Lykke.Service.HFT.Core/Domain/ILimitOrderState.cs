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
        decimal? Price { get; set; }
        DateTime Registered { get; set; }
        decimal RemainingVolume { get; set; }
        OrderStatus Status { get; set; }
        decimal Volume { get; set; }
        int Type { get; set; }
        decimal? LowerLimitPrice { get; set; }
        decimal? LowerPrice { get; set; }
        decimal? UpperLimitPrice { get; set; }
        decimal? UpperPrice { get; set; }
    }
}