using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using GeoAPI.Geometries;
using GeographicLib;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Projection;

namespace Sandwych.MapMatchingKit.Tests.Spatial
{
    public class CartesianSpatialOperationTest : TestBase
    {
        private readonly ISpatialOperation _spatial = new CartesianSpatialOperation();
        private readonly ICoordinateTransformation transformation4326To3395 = new Epsg4326To3395CoordinateTransformation();
        private readonly ICoordinateTransformation transformation3395To4326 = new Epsg3395To4326CoordinateTransformation();

        [Fact]
        public void TestTwoPointsAzimuth()
        {
            { //Quadrant 1
                var point1 = new Coordinate2D(1.0, 1.0);
                var point2 = new Coordinate2D(100.0, 100.0);
                Assert.Equal(45.0, _spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(45.0, _spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(45.0, _spatial.Azimuth(point1, point2, 1.0), 1);
            }
            { //Quadrant 2
                var point1 = new Coordinate2D(1.0, -1.0);
                var point2 = new Coordinate2D(100.0, -100.0);
                Assert.Equal(135.0, _spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(135.0, _spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(135.0, _spatial.Azimuth(point1, point2, 1.0), 1);
            } //Quardant 3
            {
                var point1 = new Coordinate2D(1.0, 1.0);
                var point2 = new Coordinate2D(-100.0, -100.0);
                Assert.Equal(225.0, _spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(225.0, _spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(225.0, _spatial.Azimuth(point1, point2, 1.0), 1);
            }
        }

        [Fact]
        public void TestLineAzimuth()
        {
            var reyk = new Coordinate2D(-21.933333, 64.15);
            var berl = new Coordinate2D(13.408056, 52.518611);
            var mosk = new Coordinate2D(37.616667, 55.75);

            Assert.Equal(AzimuthEpsg4326(berl, mosk, true), _spatial.Azimuth(berl, mosk, 0f), 1);
            Assert.Equal(AzimuthEpsg4326(berl, mosk, false), _spatial.Azimuth(berl, mosk, 1f), 1);
            Assert.Equal(AzimuthEpsg4326(berl, reyk, true), _spatial.Azimuth(berl, reyk, 0f), 1);
            Assert.True(_spatial.Azimuth(berl, mosk, 0f) < _spatial.Azimuth(berl, mosk, 0.5)
                    && _spatial.Azimuth(berl, mosk, 0.5) < _spatial.Azimuth(berl, mosk, 1f));
        }

        [Fact]
        public void TestPathAzimuth()
        {
            var reykEpsg4326 = new Coordinate2D(-21.933333, 64.15);
            var berlEpsg4326 = new Coordinate2D(13.408056, 52.518611);
            var moskEpsg4326 = new Coordinate2D(37.616667, 55.75);

            var reykEpsg3395 = transformation4326To3395.Transform(reykEpsg4326);
            var berlEpsg3395 = transformation4326To3395.Transform(berlEpsg4326);
            var moskEpsg3395 = transformation4326To3395.Transform(moskEpsg4326);

            var p = new LineString(new Coordinate[] {
                berlEpsg3395.ToGeoAPICoordinate(),
                moskEpsg3395.ToGeoAPICoordinate(),
                reykEpsg3395.ToGeoAPICoordinate()
            });

            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, moskEpsg4326, true), _spatial.Azimuth(p, 0), 1);
            Assert.Equal(AzimuthEpsg4326(moskEpsg4326, reykEpsg3395, false), _spatial.Azimuth(p, 1), 1);
            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, moskEpsg4326, false),
                    _spatial.Azimuth(p, _spatial.Distance(berlEpsg3395, moskEpsg3395) / _spatial.Length(p)), 1);

            var c = _spatial.Interpolate(berlEpsg3395, moskEpsg3395, 0.5);
            var c4326 = transformation3395To4326.Transform(c);
            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, c4326, false),
                    _spatial.Azimuth(p, _spatial.Distance(berlEpsg3395, c) / _spatial.Length(p)), 1);

            var d = _spatial.Interpolate(moskEpsg3395, reykEpsg3395, 0.5);
            var d4326 = transformation3395To4326.Transform(d);
            Assert.Equal(AzimuthEpsg4326(moskEpsg4326, d4326, false), _spatial.Azimuth(p,
                    (_spatial.Distance(berlEpsg3395, moskEpsg3395) + _spatial.Distance(moskEpsg3395, d)) / _spatial.Length(p)), 1);
        }

        private static double AzimuthEpsg4326(in Coordinate2D a, in Coordinate2D b, bool left)
        {
            var geod = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X);
            var azi = left ? geod.azi1 : geod.azi2;
            return azi < 0 ? azi + 360 : azi;
        }

        private (Coordinate2D, Double, Double) Intercept(Coordinate2D a, Coordinate2D b, Coordinate2D c)
        {
            var iter = 1000;

            var res = (a, _spatial.Distance(a, c), 0d);

            for (var f = 1; f <= iter; ++f)
            {
                var p = _spatial.Interpolate(a, b, (double)f / iter);
                double s = _spatial.Distance(p, c);

                if (s < res.Item2)
                {
                    res.Item1 = p;
                    res.Item2 = s;
                    res.Item3 = (double)f / iter;
                }
            }

            return res;
        }

    }
}
