using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Middleware
{
    public class DoubleGreaterThanZeroAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return string.Format("{0} must be more than zero.", name);
        }

        public override bool IsValid(object value)
        {
            return value as double? > double.Epsilon;
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (IsValid(value))
                return ValidationResult.Success;
            else
                return new ValidationResult(
                    FormatErrorMessage(validationContext.MemberName)
                );
        }
    }
}
