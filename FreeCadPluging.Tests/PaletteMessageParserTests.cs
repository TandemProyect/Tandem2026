using Xunit;

namespace FreeCadPluging.Tests
{
    public class PaletteMessageParserTests
    {
        [Fact]
        public void ParseAction_UsesTool_WhenModeIsNull()
        {
            var json = "{\"tool\":\"wall-2d\",\"mode\":null,\"system\":null}";

            var action = FreeCadPaletteHost.ParseActionFromPaletteJson(json);

            Assert.Equal("wall-2d", action);
        }

        [Fact]
        public void ParseAction_UsesAction_WhenPresent()
        {
            var json = "{\"action\":\"show-home\",\"tool\":\"wall-2d\"}";

            var action = FreeCadPaletteHost.ParseActionFromPaletteJson(json);

            Assert.Equal("show-home", action);
        }

        [Fact]
        public void ParseAction_UsesTool_WhenActionAndCommandAreMissing()
        {
            var json = "{\"tool\":\"wall-2d\"}";

            var action = FreeCadPaletteHost.ParseActionFromPaletteJson(json);

            Assert.Equal("wall-2d", action);
        }
    }
}
