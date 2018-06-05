using Lykke.Service.HFT.Core.Strings;

namespace Lykke.Service.HFT.Core.Domain
{
    public class ResponseModel
    {
        public ErrorModel Error { get; set; }

        public enum ErrorCodeType
        {
            InvalidInputField = 0,
            BadRequest = 400,
            LowBalance = 401,
            AlreadyProcessed = 402,
            UnknownAsset = 410,
            NoLiquidity = 411,
            NotEnoughFunds = 412,
            Dust = 413,
            ReservedVolumeHigherThanBalance = 414,
            NotFound = 415,
            BalanceLowerThanReserved = 416,
            LeadToNegativeSpread = 417,
            InvalidFee = 419,
            Duplicate = 420,
            InvalidPrice = 421,
            Replaced = 422,
            NotFoundPrevious = 423,
            Runtime = 500,
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
            return new ResponseModel
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message ?? GetErrorMessage(errorCodeType)
                }
            };
        }

        private static readonly ResponseModel OkInstance = new ResponseModel();

        public static ResponseModel CreateOk()
        {
            return OkInstance;
        }

        protected static string GetErrorMessage(ErrorCodeType code)
        {
            switch (code)
            {
                case ErrorCodeType.LowBalance:
                    return ErrorMessages.LowBalance;
                case ErrorCodeType.AlreadyProcessed:
                    return ErrorMessages.AlreadyProcessed;
                case ErrorCodeType.UnknownAsset:
                    return ErrorMessages.UnknownAsset;
                case ErrorCodeType.NoLiquidity:
                    return ErrorMessages.NoLiquidity;
                case ErrorCodeType.NotEnoughFunds:
                    return ErrorMessages.NotEnoughFunds;
                case ErrorCodeType.Dust:
                    return ErrorMessages.Dust;
                case ErrorCodeType.ReservedVolumeHigherThanBalance:
                    return ErrorMessages.ReservedVolumeHigherThanBalance;
                case ErrorCodeType.NotFound:
                    return ErrorMessages.NotFound;
                case ErrorCodeType.BalanceLowerThanReserved:
                    return ErrorMessages.BalanceLowerThanReserved;
                case ErrorCodeType.LeadToNegativeSpread:
                    return ErrorMessages.LeadToNegativeSpread;
                case ErrorCodeType.Runtime:
                    return ErrorMessages.RuntimeError;
                case ErrorCodeType.NotFoundPrevious:
                    return ErrorMessages.NotFoundPrevious;
                case ErrorCodeType.Replaced:
                    return ErrorMessages.Replaced;
                case ErrorCodeType.InvalidPrice:
                    return ErrorMessages.InvalidPrice;
                case ErrorCodeType.Duplicate:
                    return ErrorMessages.Duplicate;
                case ErrorCodeType.InvalidFee:
                    return ErrorMessages.InvalidFee;
                case ErrorCodeType.BadRequest:
                    return ErrorMessages.BadRequest;
                case ErrorCodeType.InvalidInputField:
                    return ErrorMessages.InvalidInputField;
                default:
                    return string.Format(ErrorMessages.RuntimeErrorX, (int)code);
            }
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
            return new ResponseModel<T>
            {
                Error = new ErrorModel
                {
                    Code = errorCodeType,
                    Message = message ?? GetErrorMessage(errorCodeType)
                }
            };
        }
    }
}
