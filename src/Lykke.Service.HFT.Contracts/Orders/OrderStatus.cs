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
        /// Failed: Not enough funds on account
        /// </summary>
        NotEnoughFunds = 4,

        /// <summary>
        /// Failed: No liquidity
        /// </summary>
        NoLiquidity = 5,

        /// <summary>
        /// Failed: Unknown asset
        /// </summary>
        UnknownAsset = 6,

        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 7,

        /// <summary>
        /// Failed: Lead to negative spread
        /// </summary>
        LeadToNegativeSpread = 8,

        /// <summary>
        /// Failed: Invalid price accuracy
        /// </summary>
        InvalidPriceAccuracy = 9,

        /// <summary>
        /// Order has been rejected
        /// </summary>
        Rejected = 10,

        /// <summary>
        /// Failed: The low balance
        /// </summary>
        LowBalance = 401,

        /// <summary>
        /// Failed: Already processed
        /// </summary>
        AlreadyProcessed = 402,

        /// <summary>
        /// Failed: Disabled asset
        /// </summary>
        DisabledAsset = 403,

        /// <summary>
        /// Failed: Too low volume
        /// </summary>
        Dust = 413,

        /// <summary>
        /// Failed: Reserved volume higher than balance
        /// </summary>
        ReservedVolumeHigherThanBalance = 414,

        /// <summary>
        /// Failed: Not found
        /// </summary>
        NotFound = 415,

        /// <summary>
        /// Failed: Reserved volume lower than balance
        /// </summary>
        BalanceLowerThanReserved = 416,

        /// <summary>
        /// Failed: Too small volume
        /// </summary>
        TooSmallVolume = 418,

        /// <summary>
        /// Order was replaced
        /// </summary>
        Replaced = 422,
        
        /// <summary>
        /// Failed: Invalid fee
        /// </summary>
        InvalidFee = 419,
        
        /// <summary>
        /// Failed: Invalid price
        /// </summary>
        InvalidPrice = 420,
        
        /// <summary>
        /// Failed: Previous order not found
        /// </summary>
        NotFoundPrevious = 422,
        
        /// <summary>
        /// Duplicate order
        /// </summary>
        Duplicate = 430,
        
        /// <summary>
        /// Failed: Invalid volume accuracy
        /// </summary>
        InvalidVolumeAccuracy = 431,
        
        /// <summary>
        /// Failed: Invalid volume
        /// </summary>
        InvalidVolume = 434,
        
        /// <summary>
        /// Failed: Too high price deviation
        /// </summary>
        TooHighPriceDeviation = 435,

        /// <summary>
        /// Failed: Invalid order value
        /// </summary>
        InvalidOrderValue = 436,

        /// <summary>
        /// Unexpected status code
        /// </summary>
        Runtime = 500
    }

    /// <summary>
    /// Extension methods for <see cref="OrderStatus"/>
    /// </summary>
    [UsedImplicitly]
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