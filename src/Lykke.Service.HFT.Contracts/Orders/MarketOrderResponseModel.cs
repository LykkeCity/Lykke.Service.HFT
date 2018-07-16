using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Response model for placing new market orders.
    /// </summary>
    [PublicAPI]
    public class MarketOrderResponseModel
    {
        /// <summary>
        /// The (average) price for which the market order was settled.
        /// </summary>
        public double Price { get; set; }
    }
}