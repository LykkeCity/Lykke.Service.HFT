using Lykke.Service.HFT.Core.Validation;
using System;
using Xunit;

namespace Lykke.Service.HFT.Tests
{
    public class CancelAfterValidationRuleTest
    {
        [Fact]
        public void CancelAfterInFuture_IsValid()
        {
            Assert.True(CancelAfterValidationRule.IsValid(DateTime.UtcNow.AddSeconds(1)));
        }

        [Fact]
        public void CancelAfterPast_NotValid()
        {
            Assert.False(CancelAfterValidationRule.IsValid(DateTime.UtcNow.AddSeconds(-1)));
        }
    }
}
