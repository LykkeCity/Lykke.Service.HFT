using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Contracts.Validators
{
    /// <summary>
    /// Checks if double value is greater then zero.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    internal class DoubleGreaterThanZeroAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
            => $"{name} must be more than zero.";

        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            // Nullable should be checked by required flag.
            if (value == null)
            {
                return true;
            }

            if (value as double? > double.Epsilon) return true;
            return false;
        }

        /// <inheritdoc />
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
