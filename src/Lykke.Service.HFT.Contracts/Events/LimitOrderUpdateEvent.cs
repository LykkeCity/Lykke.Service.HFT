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
        public OrderStatus Status { get; set; }
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

    public enum OrderStatus
    {
        /// <summary>
        /// Initial status, limit order is going to be processed.
        /// </summary>
        Pending,
        /// <summary>
        /// Limit order in order book.
        /// </summary>
        InOrderBook,
        /// <summary>
        /// Partially matched.
        /// </summary>
        Processing,
        /// <summary>
        /// Fully matched.
        /// </summary>
        Matched,
        /// <summary>
        /// Not enough funds on account.
        /// </summary>
        NotEnoughFunds,
        /// <summary>
        /// No liquidity.
        /// </summary>
        NoLiquidity,
        /// <summary>
        /// Unknown asset.
        /// </summary>
        UnknownAsset,
        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// Lead to negative spread
        /// </summary>
        LeadToNegativeSpread,
        ReservedVolumeGreaterThanBalance
    }
}
