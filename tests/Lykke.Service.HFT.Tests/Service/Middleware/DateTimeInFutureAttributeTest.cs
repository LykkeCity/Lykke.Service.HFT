using Lykke.Service.HFT.Middleware;
using System;
using Xunit;

namespace Lykke.Service.HFT.Tests.Service.Middleware
{
    public class DateTimeInFutureAttributeTest
    {
        [Fact]
        public void DateTimeInFuture_IsValid()
        {
            var sot = new DateTimeInFutureAttribute();
            Assert.True(sot.IsValid(DateTime.Now.AddSeconds(1)));
        }

        [Fact]
        public void MissingDataTime_IsValid()
        {
            var sot = new DateTimeInFutureAttribute();
            Assert.True(sot.IsValid(default(DateTime?)));
        }

        [Fact]
        public void DateTimePast_NotValid()
        {
            var sot = new DateTimeInFutureAttribute();
            Assert.False(sot.IsValid(DateTime.Now.AddSeconds(-1)));
        }
    }
}
