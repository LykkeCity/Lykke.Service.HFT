using JetBrains.Annotations;

namespace Lykke.Service.HFT.Contracts.Orders
{
    /// <summary>
    /// Status of the limit order.
    /// </summary>
    [PublicAPI]
    public enum OrderStatus
    {
        /// <summary>
        /// Order request is not acknowledged.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Initial status, limit order in order book
        /// </summary>
        InOrderBook = 1,

        /// <summary>
        /// Partially matched
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Fully matched
        /// </summary>
        Matched = 3,

        /// <summary>
        /// Not enough funds on account
        /// </summary>
        NotEnoughFunds = 4,

        /// <summary>
        /// No liquidity
        /// </summary>
        NoLiquidity = 5,

        /// <summary>
        /// Unknown asset
        /// </summary>
        UnknownAsset = 6,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// Lead to negative spread
        /// </summary>
        LeadToNegativeSpread = 8,

        /// <summary>
        /// Invalid price accuracy
        /// </summary>
        InvalidPriceAccuracy = 9,

        /// <summary>
        /// Order has been rejected
        /// </summary>
        Rejected = 10,

        /// <summary>
        /// Reserved volume greater than balance
        /// </summary>
        ReservedVolumeGreaterThanBalance = 414,

        /// <summary>
        /// Too small volume
        /// </summary>
        TooSmallVolume = 418,

        /// <summary>
        /// Order was replaced
        /// </summary>
        Replaced = 422,

        /// <summary>
        /// Unexpected status code
        /// </summary>
        Runtime = 500
    }

    /// <summary>
    /// Extension methods for <see cref="OrderStatus"/>
    /// </summary>
    public static class OrderStatusMixin
    {
        /// <summary>
        /// Determines whether this order is rejected.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the specified status is rejected; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRejected(this OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.NotEnoughFunds:
                case OrderStatus.NoLiquidity:
                case OrderStatus.UnknownAsset:
                case OrderStatus.LeadToNegativeSpread:
                case OrderStatus.ReservedVolumeGreaterThanBalance:
                case OrderStatus.TooSmallVolume:
                case OrderStatus.InvalidPriceAccuracy:
                case OrderStatus.Runtime:
                case OrderStatus.Rejected:
                    return true;
                case OrderStatus.Pending:
                case OrderStatus.InOrderBook:
                case OrderStatus.Processing:
                case OrderStatus.Matched:
                case OrderStatus.Cancelled:
                case OrderStatus.Replaced:
                    return false;
                default:
                    return true;
            }
        }
    }
}