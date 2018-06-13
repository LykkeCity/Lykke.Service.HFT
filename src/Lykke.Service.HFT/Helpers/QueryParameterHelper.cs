using Lykke.Service.HFT.Core.Domain;

namespace Lykke.Service.HFT.Helpers
{
    public static class QueryParameterHelper
    {
        public static ResponseModel<int> ValidateAndGetValue(this int? paramValue, string parameter, int maxValue, int defaultValue)
        {
            var value = paramValue.GetValueOrDefault(defaultValue);

            if (value > maxValue)
            {
                return ResponseModel<int>.CreateInvalidFieldError(parameter, $"{parameter} '{value}' is too big, maximum is '{maxValue}'.");
            }

            if (value < 0)
            {
                return ResponseModel<int>.CreateInvalidFieldError(parameter, $"{parameter} cannot be less than zero.");
            }

            if (value == 0)
            {
                value = defaultValue;
            }

            return ResponseModel<int>.CreateOk(value);
        }
    }
}