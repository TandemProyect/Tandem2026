using Xunit;

namespace OpenCadStudioPluging.Tests
{
    public class OpenCadPaletteHostTests
    {
        [Fact]
        public void ParseAction_UsesTool_WhenModeIsNull()
        {
            var json = "{\"tool\":\"wall-2d\",\"mode\":null,\"system\":null}";

            var action = OpenCadPaletteHost.ParseActionFromPaletteJson(json);

            Assert.Equal("wall-2d", action);
        }

        [Fact]
        public void ParseAction_UsesAction_WhenPresent()
        {
            var json = "{\"action\":\"show-home\",\"tool\":\"wall-2d\"}";

            var action = OpenCadPaletteHost.ParseActionFromPaletteJson(json);

            Assert.Equal("show-home", action);
        }
    }
}
