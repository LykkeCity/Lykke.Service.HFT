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
        public decimal Volume { get; set; }
        public decimal? Price { get; set; }
        public decimal RemainingVolume { get; set; }
        public DateTime? LastMatchTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Registered { get; set; }
        public int Type { get; set; } = 0;
        public decimal? LowerLimitPrice { get; set; }
        public decimal? LowerPrice { get; set; }
        public decimal? UpperLimitPrice { get; set; }
        public decimal? UpperPrice { get; set; }
    }
}
