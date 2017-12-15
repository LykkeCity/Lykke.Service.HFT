using System;

namespace Lykke.Service.HFT.Models
{
    public class HistoryTradeModel
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public string LimitOrderId { get; set; }
        public string MarketOrderId { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public string AssetId { get; set; }
        public string State { get; set; }
    }
}