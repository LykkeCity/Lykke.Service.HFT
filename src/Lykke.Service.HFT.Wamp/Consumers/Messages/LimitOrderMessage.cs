using System;

namespace Lykke.Service.HFT.Wamp.Consumers.Messages
{
    public class LimitOrderMessage
    {
        public LimitOrder[] Orders { get; set; }

        public class LimitOrder
        {
            public Order Order { get; set; }
            public Trade[] Trades { get; set; }
        }
        public class Order
        {
            public double? Price { get; set; }
            public double RemainingVolume { get; set; }
            public DateTime? LastMatchTime { get; set; }
            public string Id { get; set; }
            public string ExternalId { get; set; }
            public string AssetPairId { get; set; }
            public string ClientId { get; set; }
            public double Volume { get; set; }
            public OrderStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime Registered { get; set; }
        }
        public class Trade
        {
            public string ClientId { get; set; }
            public string Asset { get; set; }
            public double Volume { get; set; }
            public double? Price { get; set; }
            public DateTime Timestamp { get; set; }
            public string OppositeOrderId { get; set; }
            public string OppositeOrderExternalId { get; set; }
            public string OppositeAsset { get; set; }
            public string OppositeClientId { get; set; }
            public double OppositeVolume { get; set; }
        }
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
        LeadToNegativeSpread
    }
}
