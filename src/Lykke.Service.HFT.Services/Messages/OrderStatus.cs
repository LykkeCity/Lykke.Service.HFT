namespace Lykke.Service.HFT.Services.Messages
{
	public enum OrderStatus
	{
		Unknown,
		//Init status, limit order in order book
		InOrderBook
		//Partially matched
		, Processing
		//Fully matched
		, Matched
		//Not enough funds on account
		, NotEnoughFunds
		//No liquidity
		, NoLiquidity
		//Unknown asset
		, UnknownAsset
		//Cancelled
		, Cancelled
	}
}
