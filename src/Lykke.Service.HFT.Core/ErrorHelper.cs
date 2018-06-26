using Lykke.Service.HFT.Contracts;
using Lykke.Service.HFT.Core.Strings;

namespace Lykke.Service.HFT.Core
{
    public static class ErrorHelper
    {
        public static string GetErrorMessage(this ErrorCodeType code)
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
}