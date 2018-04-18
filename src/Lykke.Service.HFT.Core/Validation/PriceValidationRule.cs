namespace Lykke.Service.HFT.Core.Validation
{
    public static class PriceValidationRule
    {
        public static bool IsValid(double price)
            => price > double.Epsilon;
    }
}
