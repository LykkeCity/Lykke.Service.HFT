﻿namespace Lykke.Service.HFT.Wamp.Messages
{
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