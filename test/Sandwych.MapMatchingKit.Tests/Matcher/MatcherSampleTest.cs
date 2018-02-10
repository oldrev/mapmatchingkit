using Sandwych.MapMatchingKit.Matcher;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Tests.Matcher
{
    public class MatcherSampleTest : TestBase
    {
        [Fact]
        public void TestAzimuth()
        {
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), -0.1f);
                Assert.Equal(359.9, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), -359.9f);
                Assert.Equal(0.1, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), -360.1f);
                Assert.Equal(359.9, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), 360f);
                Assert.Equal(0.0, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), 360.1f);
                Assert.Equal(0.1, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), 720.1f);
                Assert.Equal(0.1, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), -719.9f);
                Assert.Equal(0.1, sample.Azimuth, 1);
            }
            {
                var sample = new MatcherSample<int>(0, 0L, new Coordinate2D(1, 1), -720.1f);
                Assert.Equal(359.9, sample.Azimuth, 1);
            }
        }

    }
}
