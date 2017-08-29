using System.Collections.Generic;
using Lykke.Service.HFT.Core.Strings;

namespace Lykke.Service.HFT.Core.Domain
{
	public class ResponseModel
	{
		protected static readonly Dictionary<ErrorCodeType, string> StatusCodesMap = new Dictionary<ErrorCodeType, string>
		{
			{ErrorCodeType.Ok, string.Empty},
			{ErrorCodeType.LowBalance, ErrorMessages.LowBalance},
			{ErrorCodeType.AlreadyProcessed, ErrorMessages.AlreadyProcessed},
			{ErrorCodeType.UnknownAsset, ErrorMessages.UnknownAsset},
			{ErrorCodeType.NoLiquidity, ErrorMessages.NoLiquidity},
			{ErrorCodeType.NotEnoughFunds, ErrorMessages.NotEnoughFunds},
			{ErrorCodeType.Dust, ErrorMessages.Dust},
			{ErrorCodeType.ReservedVolumeHigherThanBalance, ErrorMessages.ReservedVolumeHigherThanBalance},
			{ErrorCodeType.NotFound, ErrorMessages.NotFound},
			{ErrorCodeType.RuntimeError, ErrorMessages.RuntimeError}
		};
		public ErrorModel Error { get; set; }

		public enum ErrorCodeType
		{
			InvalidInputField = 0,
			Ok = 200,
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

		public class ErrorModel
		{
			public ErrorCodeType Code { get; set; }
			/// <summary>
			/// In case ErrorCoderType = 0
			/// </summary>
			public string Field { get; set; }
			/// <summary>
			/// Localized Error message
			/// </summary>
			public string Message { get; set; }
		}

		public static ResponseModel CreateInvalidFieldError(string field, string message)
		{
			return new ResponseModel
			{
				Error = new ErrorModel
				{
					Code = ErrorCodeType.InvalidInputField,
					Field = field,
					Message = message
				}
			};
		}

		public static ResponseModel CreateFail(ErrorCodeType errorCodeType, string message = null)
		{
			if (message == null)
			{
				StatusCodesMap.TryGetValue(errorCodeType, out message);
			}

			return new ResponseModel
			{
				Error = new ErrorModel
				{
					Code = errorCodeType,
					Message = message
				}
			};
		}

		private static readonly ResponseModel OkInstance = new ResponseModel();

		public static ResponseModel CreateOk()
		{
			return OkInstance;
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

		public new static ResponseModel<T> CreateInvalidFieldError(string field, string message)
		{
			return new ResponseModel<T>
			{
				Error = new ErrorModel
				{
					Code = ErrorCodeType.InvalidInputField,
					Field = field,
					Message = message
				}
			};
		}

		public new static ResponseModel<T> CreateFail(ErrorCodeType errorCodeType, string message = null)
		{
			if (message == null)
			{
				StatusCodesMap.TryGetValue(errorCodeType, out message);
			}

			return new ResponseModel<T>
			{
				Error = new ErrorModel
				{
					Code = errorCodeType,
					Message = message
				}
			};
		}
	}
}
