using Lykke.Service.HFT.Core.Validation;
using Xunit;

namespace Lykke.Service.HFT.Tests.Core.Validation
{
    public class PriceValidationRuleTest
    {
        [Fact]
        public void PriceGreaterEpsilon_IsValid()
        {
            Assert.True(PriceValidationRule.IsValid(double.Epsilon * 2));
        }

        [Fact]
        public void PricePositive_IsValid()
        {
            Assert.True(PriceValidationRule.IsValid(1.1));
        }

        [Fact]
        public void PriceEpsilon_NotValid()
        {
            Assert.False(PriceValidationRule.IsValid(double.Epsilon));
        }

        [Fact]
        public void PriceZero_NotValid()
        {
            Assert.False(PriceValidationRule.IsValid(0d));
        }

        [Fact]
        public void PriceNegative_NotValid()
        {
            Assert.False(PriceValidationRule.IsValid(-1d));
        }
    }
}
