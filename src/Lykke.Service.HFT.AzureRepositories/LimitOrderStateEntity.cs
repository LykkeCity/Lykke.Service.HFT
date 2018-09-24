using System;
using Lykke.Service.HFT.Contracts.Orders;
using Lykke.Service.HFT.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.HFT.AzureRepositories
{
    public class LimitOrderStateEntity : TableEntity, ILimitOrderState
    {
        public string AssetPairId { get; set; }
        public string ClientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Id => Guid.Parse(RowKey);
        public DateTime? LastMatchTime { get; set; }
        public decimal? Price { get; set; }
        public DateTime Registered { get; set; }
        public decimal RemainingVolume { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusString
        {
            get => Status.ToString();
            set => Status = Enum.Parse<OrderStatus>(value);
        }
        public decimal Volume { get; set; }
        public int Type { get; set; }
        public decimal? LowerLimitPrice { get; set; }
        public decimal? LowerPrice { get; set; }
        public decimal? UpperLimitPrice { get; set; }
        public decimal? UpperPrice { get; set; }
    }
}
