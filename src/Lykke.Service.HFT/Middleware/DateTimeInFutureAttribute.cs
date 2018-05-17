using Lykke.Service.HFT.Core.Validation;
using Lykke.Service.HFT.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Middleware
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DateTimeInFutureAttribute : ValidationAttribute
    {
        public DateTimeInFutureAttribute()
        {
            ErrorMessageResourceType = typeof(Resources);
            ErrorMessageResourceName = nameof(Resources.DateInFuture);
        }

        public override bool IsValid(object value)
            => !(value is DateTime date) || CancelAfterValidationRule.IsValid(date);
    }
}
