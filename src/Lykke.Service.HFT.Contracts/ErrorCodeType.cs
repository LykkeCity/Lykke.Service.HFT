namespace Lykke.Service.HFT.Contracts
{
    /// <summary>
    /// Well known error codes enumeration
    /// </summary>
    public enum ErrorCodeType
    {
        /// <summary>
        /// One of the input parameters was invalid.
        /// </summary>
        InvalidInputField = 0,

        /// <summary>
        /// Bad request
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// Low balance
        /// </summary>
        LowBalance = 401,

        /// <summary>
        /// Order was already processed
        /// </summary>
        AlreadyProcessed = 402,

        /// <summary>
        /// Unknown asset
        /// </summary>
        UnknownAsset = 410,

        /// <summary>
        /// Not enough liquidity
        /// </summary>
        NoLiquidity = 411,

        /// <summary>
        /// Not enough funds
        /// </summary>
        NotEnoughFunds = 412,

        /// <summary>
        /// Volume too small
        /// </summary>
        Dust = 413,

        /// <summary>
        /// Reserved volume is higher than balance
        /// </summary>
        ReservedVolumeHigherThanBalance = 414,

        /// <summary>
        /// Requested item not found
        /// </summary>
        NotFound = 415,

        /// <summary>
        /// Balance is lower than reserved
        /// </summary>
        BalanceLowerThanReserved = 416,

        /// <summary>
        /// Orders leads to negative spread (self-trade prevention)
        /// </summary>
        LeadToNegativeSpread = 417,

        /// <summary>
        /// Invalid trade fee
        /// </summary>
        InvalidFee = 419,

        /// <summary>
        /// Duplicate request
        /// </summary>
        Duplicate = 420,

        /// <summary>
        /// Invalid price
        /// </summary>
        InvalidPrice = 421,

        /// <summary>
        /// Item was replaced
        /// </summary>
        Replaced = 422,

        /// <summary>
        /// Previous not found
        /// </summary>
        NotFoundPrevious = 423,

        /// <summary>
        /// Runtime error
        /// </summary>
        Runtime = 500,
    }
}