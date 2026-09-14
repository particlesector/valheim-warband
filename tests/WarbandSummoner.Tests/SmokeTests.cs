using WarbandSummoner.Core;
using Xunit;

namespace WarbandSummoner.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void CoreAssemblyLoads()
        {
            Assert.False(string.IsNullOrEmpty(CoreInfo.Version));
        }
    }
}
