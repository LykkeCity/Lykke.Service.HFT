using System;

namespace Lykke.Service.HFT.Models
{
    public class HistoryTradeModel
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public OperationsHistory.AutorestClient.Models.HistoryOperationState State { get; set; }
        public double Amount { get; set; }
        public string Asset { get; set; }
        public string AssetPair { get; set; }
        public double? Price { get; set; }
        public Fee Fee { get; set; }
    }

    public class Fee
    {
        public double Amount { get; set; }
        public OperationsHistory.AutorestClient.Models.FeeType Type { get; set; }
    }
}