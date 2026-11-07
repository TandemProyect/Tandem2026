using Xunit;

namespace OpenCadStudioPluging.Tests
{
    public class BridgeActionMapTests
    {
        [Fact]
        public void MapToBridgeArguments_Wall2d_UsesTandemMuro2dCommand()
        {
            var args = BridgeActionMap.MapToBridgeArguments("wall-2d");

            Assert.NotNull(args);
            Assert.Equal("ui-run", args[0]);
            Assert.Equal("TANDEM_MURO2D", args[1]);
        }

        [Fact]
        public void MapToBridgeArguments_Muro2dAlias_UsesTandemMuro2dCommand()
        {
            var args = BridgeActionMap.MapToBridgeArguments("muro-2d");

            Assert.NotNull(args);
            Assert.Equal("ui-run", args[0]);
            Assert.Equal("TANDEM_MURO2D", args[1]);
        }

        [Fact]
        public void MapToBridgeArguments_LinesMode_UsesTandemMuro2dCommand()
        {
            var args = BridgeActionMap.MapToBridgeArguments("lines");

            Assert.NotNull(args);
            Assert.Equal("ui-run", args[0]);
            Assert.Equal("TANDEM_MURO2D", args[1]);
        }
    }
}
