namespace Lykke.Service.HFT.Core.Domain
{
    public enum OrderStatus
    {
        // values 4, 5, 6, 8 are used for DB compatibility reasons only; it should be ME related values

        /// <summary>
        /// Order request is not acknowledged.
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Init status, limit order in order book
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
        /// Reserved volume greater than balance
        /// </summary>
        ReservedVolumeGreaterThanBalance = 414,
        /// <summary>
        /// Too small volume
        /// </summary>
        TooSmallVolume = 418,
        /// <summary>
        /// Unexpected status code
        /// </summary>
        Runtime = 500
    }

    public static class OrderStatusMixin
    {
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
                    return true;
                case OrderStatus.Pending:
                case OrderStatus.InOrderBook:
                case OrderStatus.Processing:
                case OrderStatus.Matched:
                case OrderStatus.Cancelled:
                    return false;
                default:
                    return true;
            }
        }
    }
}
