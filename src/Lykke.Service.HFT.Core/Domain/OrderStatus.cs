namespace Lykke.Service.HFT.Core.Domain
{
	public enum OrderStatus
	{
		/// <summary>
		/// Init status, limit order is going to be processed.
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
		Cancelled
	}
}
