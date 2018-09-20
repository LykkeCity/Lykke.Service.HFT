using System;
using Lykke.Service.HFT.Contracts.Orders;

namespace Lykke.Service.HFT.Core.Domain
{
    public class LimitOrderState : IHasId, ILimitOrderState
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public OrderStatus Status { get; set; }
        public string AssetPairId { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public double RemainingVolume { get; set; }
        public DateTime? LastMatchTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Registered { get; set; }
        public int Type { get; set; } = 0;
        public double? LowerLimitPrice { get; set; }
        public double? LowerPrice { get; set; }
        public double? UpperLimitPrice { get; set; }
        public double? UpperPrice { get; set; }
    }
}
