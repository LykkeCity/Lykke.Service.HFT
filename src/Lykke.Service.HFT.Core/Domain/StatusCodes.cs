namespace Lykke.Service.HFT.Core.Domain
{
	public enum StatusCodes
	{
		Ok = 0,
		LowBalance = 401,
		AlreadyProcessed = 402,
		UnknownAsset = 410,
		NoLiquidity = 411,
		NotEnoughFunds = 412,
		Dust = 413,
		ReservedVolumeHigherThanBalance = 414,
		NotFound = 415,
		RuntimeError = 500
	}
}