using System;

namespace Lykke.Service.HFT.Contracts.Events
{
    public class LimitOrderUpdateEvent
    {
        public Order Order { get; set; }
        public Trade[] Trades { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string AssetPairId { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public double RemainingVolume { get; set; }
        public DateTime? LastMatchTime { get; set; }
    }

    public class Trade
    {
        public string Asset { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public DateTime Timestamp { get; set; }
        public string OppositeAsset { get; set; }
        public double OppositeVolume { get; set; }
    }
}
