using System;

namespace Lykke.Service.HFT.Models
{
    public class LimitOrderState
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string AssetPairId { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public double RemainingVolume { get; set; }
        public DateTime? LastMatchTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
