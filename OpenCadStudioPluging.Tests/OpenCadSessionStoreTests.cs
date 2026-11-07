using Xunit;

namespace OpenCadStudioPluging.Tests
{
    public class OpenCadSessionStoreTests
    {
        [Fact]
        public void IsSameBootMarker_True_ForExactMatch()
        {
            Assert.True(OpenCadSessionStore.IsSameBootMarker("202611061430", "202611061430"));
        }

        [Fact]
        public void IsSameBootMarker_False_WhenDifferent()
        {
            Assert.False(OpenCadSessionStore.IsSameBootMarker("202611061430", "202611061431"));
        }
    }
}
