using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Spatial.Projection;

namespace Sandwych.MapMatchingKit.Tests.Spatial.Projection
{
    public class Epsg4326To3395CoordinateTransformationTest
    {
        [Fact]
        public void TransformTest()
        {
            var transform = new Epsg4326To3395CoordinateTransformation();
            var fromCoord = new Coordinate2D(102.709887, 25.054263);
            var toCoord = transform.Transform(fromCoord);
            Assert.Equal(11433612.32, toCoord.X, 2);
            Assert.Equal(2864322.39, toCoord.Y, 2);
        }
    }
}
