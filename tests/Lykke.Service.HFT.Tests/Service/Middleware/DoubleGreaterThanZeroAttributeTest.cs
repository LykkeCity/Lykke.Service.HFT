using Lykke.Service.HFT.Middleware;
using Xunit;

namespace Lykke.Service.HFT.Tests.Service.Middleware
{
    public class DoubleGreaterThanZeroAttributeTest
    {
        [Fact]
        public void DoublePositive_IsValid()
        {
            var sot = new DoubleGreaterThanZeroAttribute();
            Assert.True(sot.IsValid(1.1));
        }

        [Fact]
        public void DoubleZero_NotValid()
        {
            var sot = new DoubleGreaterThanZeroAttribute();
            Assert.False(sot.IsValid(0d));
        }

        [Fact]
        public void DoubleNegative_NotValid()
        {
            var sot = new DoubleGreaterThanZeroAttribute();
            Assert.False(sot.IsValid(-1d));
        }

        [Fact]
        public void MissingDouble_Ignored()
        {
            var sot = new DoubleGreaterThanZeroAttribute();
            Assert.True(sot.IsValid(null));
        }
    }
}
