using Xunit;

namespace OpenCadStudioPluging.Tests
{
    public class MvcWall2dStepClientTests
    {
        [Fact]
        public void ParsePointsArg_ParsesValidPoints_AndDefaultsMissingZToZero()
        {
            var points = MvcWall2dStepClient.ParsePointsArg("0,0;10.5,2.25,3");

            Assert.Equal(2, points.Count);
            Assert.Equal(0, points[0].X);
            Assert.Equal(0, points[0].Y);
            Assert.Equal(0, points[0].Z);
            Assert.Equal(10.5, points[1].X);
            Assert.Equal(2.25, points[1].Y);
            Assert.Equal(3, points[1].Z);
        }

        [Fact]
        public void ParsePointsArg_IgnoresInvalidChunks()
        {
            var points = MvcWall2dStepClient.ParsePointsArg("0,0;bad;1,2,3;4,x,0");

            Assert.Equal(2, points.Count);
            Assert.Equal(0, points[0].X);
            Assert.Equal(0, points[0].Y);
            Assert.Equal(1, points[1].X);
            Assert.Equal(2, points[1].Y);
            Assert.Equal(3, points[1].Z);
        }

        [Fact]
        public void ParsePointsArg_EmptyInput_ReturnsEmptyList()
        {
            var points = MvcWall2dStepClient.ParsePointsArg("   ");
            Assert.Empty(points);
        }

        [Fact]
        public void ParseAxisSegmentsArg_ParsesValidSegments()
        {
            var segments = MvcWall2dStepClient.ParseAxisSegmentsArg("0,0,0,10,0,0;10,0,0,10,5,0");

            Assert.Equal(2, segments.Count);
            Assert.Equal(0, segments[0].Start.X);
            Assert.Equal(10, segments[0].End.X);
            Assert.Equal(0, segments[0].End.Y);
            Assert.Equal(10, segments[1].Start.X);
            Assert.Equal(5, segments[1].End.Y);
        }

        [Fact]
        public void ParseAxisSegmentsArg_IgnoresInvalidChunks()
        {
            var segments = MvcWall2dStepClient.ParseAxisSegmentsArg("0,0,0,10,0,0;bad;1,2,3,4,5");

            Assert.Single(segments);
            Assert.Equal(0, segments[0].Start.X);
            Assert.Equal(10, segments[0].End.X);
        }

        [Fact]
        public void ParseAxisSegmentsArg_EmptyInput_ReturnsEmptyList()
        {
            var segments = MvcWall2dStepClient.ParseAxisSegmentsArg(" ");
            Assert.Empty(segments);
        }
    }
}
