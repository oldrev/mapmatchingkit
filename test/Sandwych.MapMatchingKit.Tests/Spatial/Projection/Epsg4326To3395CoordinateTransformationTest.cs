using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Spatial.Projection;

namespace Sandwych.MapMatchingKit.Tests.Spatial.Projection
{
    public class Epsg3395To4326CoordinateTransformationTest
    {
        [Fact]
        public void TransformTest()
        {
            var transform = new Epsg3395To4326CoordinateTransformation();
            var fromCoord = new Coordinate2D(11433612.32, 2864322.39);
            var toCoord = transform.Transform(fromCoord);
            Assert.Equal(102.709887, toCoord.X, 6);
            Assert.Equal(25.054263, toCoord.Y, 6);
        }
    }
}
