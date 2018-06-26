namespace Lykke.Service.HFT.Contracts.Events
{
    /// <summary>
    /// Wamp real-time event when limit order is updated.
    /// </summary>
    public class LimitOrderUpdateEvent
    {
        /// <summary>
        /// The updated limit order.
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// The associated trades.
        /// </summary>
        public Trade[] Trades { get; set; }
    }
}
