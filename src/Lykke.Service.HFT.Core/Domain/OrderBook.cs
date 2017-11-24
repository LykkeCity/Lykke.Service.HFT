using System;
using System.Collections.Generic;

namespace Lykke.Service.HFT.Core.Domain
{
    public class OrderBook
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public List<VolumePrice> Prices { get; set; } = new List<VolumePrice>();
    }

    public class VolumePrice
    {
        public double Volume { get; set; }
        public double Price { get; set; }
    }
}
