using System;
using System.Globalization;
using Xunit;

namespace OpenCadStudioPluging.Tests
{
    public class OpenCadServeProtocolTests
    {
        [Fact]
        public void BuildLegacyRunRequest_Throws_WhenCommandIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => OpenCadServeProtocol.BuildLegacyRunRequest("   "));
        }

        [Fact]
        public void BuildLineCommand_UsesInvariantCulture_DecimalDots()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("es-ES");

                var command = OpenCadServeProtocol.BuildLineCommand(1.5, 2.25, 3.75, 4.5);

                Assert.Equal("LINE 1.5,2.25 3.75,4.5", command);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Fact]
        public void TryParseReadyBanner_ReturnsFalse_WhenReadyFlagIsMissing()
        {
            var ok = OpenCadServeProtocol.TryParseReadyBanner("{\"ok\":true}", out var error);

            Assert.False(ok);
            Assert.Contains("ready:true", error);
        }

        [Fact]
        public void TryParseRunResponse_ParsesKnownFields()
        {
            var ok = OpenCadServeProtocol.TryParseRunResponse(
                "{\"ok\":true,\"status\":\"completed\",\"blocked_by\":null,\"added\":1,\"entities\":3}",
                out var response,
                out var error);

            Assert.True(ok);
            Assert.Null(error);
            Assert.Equal("completed", response.Status);
            Assert.Null(response.BlockedBy);
            Assert.Equal(1, response.Added);
            Assert.Equal(3, response.Entities);
        }
    }
}
