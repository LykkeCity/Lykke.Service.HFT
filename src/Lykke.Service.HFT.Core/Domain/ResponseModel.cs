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
		Runtime = 500
	}

	public class ResponseModel
	{
		public StatusCodes Status { get; set; }
		public string Message { get; set; }

		public static ResponseModel CreateFail(StatusCodes errorCodeType, string message)
		{
			return new ResponseModel
			{
				Status = errorCodeType,
				Message = message
			};
		}
	}

	public class ResponseModel<T> : ResponseModel
	{
		public T Result { get; set; }

		public static ResponseModel<T> CreateOk(T result)
		{
			return new ResponseModel<T>
			{
				Result = result
			};
		}

		public new static ResponseModel<T> CreateFail(StatusCodes errorCodeType, string message)
		{
			return new ResponseModel<T>
			{
				Status = errorCodeType,
				Message = message
			};
		}
	}
}
