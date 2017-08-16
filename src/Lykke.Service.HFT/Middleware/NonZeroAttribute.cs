using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.HFT.Middleware
{
	public class NonZeroAttribute : ValidationAttribute
	{
		public override string FormatErrorMessage(string name)
		{
			return string.Format("{0} must be non-zero", name);
		}

		public override bool IsValid(object value)
		{
			var zero = Convert.ChangeType(0, value.GetType());
			return !zero.Equals(value);
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
