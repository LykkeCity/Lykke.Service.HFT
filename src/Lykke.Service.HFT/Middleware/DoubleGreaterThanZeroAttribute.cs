using Lykke.Service.HFT.Core.Validation;
using Lykke.Service.HFT.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Middleware
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DoubleGreaterThanZeroAttribute : ValidationAttribute
    {
        public DoubleGreaterThanZeroAttribute()
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = nameof(Resources.ValueMustBePositive);
        }

        public override bool IsValid(object value)
            => !(value is double dValue) || PriceValidationRule.IsValid(dValue);
    }
}
