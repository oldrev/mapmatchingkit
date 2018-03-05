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
using GeoAPI.CoordinateSystems;
using NetTopologySuite.IO;

namespace Sandwych.MapMatchingKit.Tests.Spatial
{
#if CARTESIAN_SPATIAL

    public class CartesianSpatialOperationTest : AbstractSpatialOperationTest
    {
        protected override ISpatialOperation Spatial => new CartesianSpatialOperation();

        private readonly ICoordinateTransformation _transformation4326To3395 = new Epsg4326To3395CoordinateTransformation();
        private readonly ICoordinateTransformation _transformation3395To4326 = new Epsg3395To4326CoordinateTransformation();

        private readonly Coordinate2D _reyk_4326 = new Coordinate2D(-21.933333, 64.15);
        private readonly Coordinate2D _berl_4326 = new Coordinate2D(13.408056, 52.518611);
        private readonly Coordinate2D _mosk_4326 = new Coordinate2D(37.616667, 55.75);

        private readonly Coordinate2D _reyk_3395;
        private readonly Coordinate2D _berl_3395;
        private readonly Coordinate2D _mosk_3395;

        public CartesianSpatialOperationTest()
        {
            _reyk_3395 = _transformation4326To3395.Transform(_reyk_4326);
            _berl_3395 = _transformation4326To3395.Transform(_berl_4326);
            _mosk_3395 = _transformation4326To3395.Transform(_mosk_4326);
        }

        [Fact]
        public void TestDistance()
        {
            double dist_geog = Spatial.Distance(_mosk_3395, _reyk_3395);
            double dist_nts = 6889414.25774;

            Assert.InRange(dist_geog, dist_nts - 1E-5, dist_nts + 1E-5);

            dist_geog = Spatial.Distance(_berl_3395, _reyk_3395);
            dist_nts = 4655392.00544;

            Assert.InRange(dist_geog, dist_nts - 1E-5, dist_nts + 1E-5);
        }

        [Fact]
        public void TestGnomonic()
        {
            var f = Spatial.Intercept(_reyk_3395, _mosk_3395, _berl_3395);
            var p = Spatial.Interpolate(_reyk_3395, _mosk_3395, f);
            var res = Intercept(_reyk_3395, _mosk_3395, _berl_3395);

            Assert.InRange(f, res.Item3 - 0.1, res.Item3 + 0.1);
            Assert.InRange(p.X, res.Item1.X - 1, res.Item1.X + 1);
            Assert.InRange(p.Y, res.Item1.Y - 1, res.Item1.Y + 1);
        }

        [Fact]
        public void TestTwoPointsAzimuth()
        {
            { //Quadrant 1
                var point1 = new Coordinate2D(1.0, 1.0);
                var point2 = new Coordinate2D(100.0, 100.0);
                Assert.Equal(45.0, Spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(45.0, Spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(45.0, Spatial.Azimuth(point1, point2, 1.0), 1);
            }
            { //Quadrant 2
                var point1 = new Coordinate2D(1.0, -1.0);
                var point2 = new Coordinate2D(100.0, -100.0);
                Assert.Equal(135.0, Spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(135.0, Spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(135.0, Spatial.Azimuth(point1, point2, 1.0), 1);
            } //Quardant 3
            {
                var point1 = new Coordinate2D(1.0, 1.0);
                var point2 = new Coordinate2D(-100.0, -100.0);
                Assert.Equal(225.0, Spatial.Azimuth(point1, point2, 0D), 1);
                Assert.Equal(225.0, Spatial.Azimuth(point1, point2, 0.5), 1);
                Assert.Equal(225.0, Spatial.Azimuth(point1, point2, 1.0), 1);
            }
        }

        [Fact]
        public void TestLineInterception()
        {
            var wktReader = new WKTReader();
            var ab_4326 = wktReader.Read("LINESTRING(11.4047661 48.1403687,11.4053519 48.141055)") as ILineString;
            var a_4326 = ab_4326.GetPointN(0);
            var b_4326 = ab_4326.GetPointN(1);
            var a = _transformation4326To3395.Transform(a_4326.ToCoordinate2D());
            var b = _transformation4326To3395.Transform(b_4326.ToCoordinate2D());

            var points = new String[] {
                "POINT(11.406501117689324 48.14051652560591)", // East
                "POINT(11.406713245538327 48.14182906667162)", // Northeast
                "POINT(11.404923416812364 48.14258477213369)", // North
                "POINT(11.403300759321036 48.14105540093837)", // Northwest
                "POINT(11.403193249043934 48.140881120346386)", // West
                "POINT(11.40327279698731 48.13987351306362)", // Southwest
                "POINT(11.405221721600025 48.1392039845402)", // South
                "POINT(11.406255844863914 48.13963486923349)" // Southeast
            };

            foreach (var pointWkt in points)
            {
                var c_4326 = wktReader.Read(pointWkt) as IPoint;
                var c = _transformation4326To3395.Transform(c_4326.ToCoordinate2D());

                var f = Spatial.Intercept(a, b, c);
                var p = Spatial.Interpolate(a, b, f);

                var res = Intercept(a, b, c);

                var s = Spatial.Distance(p, c);

                var p_4326 = _transformation3395To4326.Transform(p);
                var s_esri = this.DistanceEpsg4326(p_4326, c_4326.ToCoordinate2D());

                Assert.InRange(f > 1 ? 1 : f < 0 ? 0 : f, res.Item3 - 0.2, res.Item3 + 0.2);
                Assert.InRange(p.X, res.Item1.X - 50, res.Item1.X + 50);
                Assert.InRange(p.Y, res.Item1.Y - 50, res.Item1.Y + 50);

                AssertEquals(s, s_esri, 10E-6);
            }
        }

        [Fact]
        public void TestPathInterception1()
        {
            var point = "POINT(1270224.85331350634805858 6099029.88927983772009611)";
            var line =
                   "LINESTRING(1269565.54136538505554199 6098373.51875618938356638, 1269565.81966411229223013 6098383.01558326091617346, 1269572.754868388408795 6098399.13193933106958866, 1269637.96582609508186579 6098513.27786348387598991, 1269650.18870618427172303 6098535.74797636084258556, 1269739.76750042568892241 6098703.18628658913075924, 1269761.16310655628331006 6098744.95074261073023081, 1269781.90192769095301628 6098793.06910624075680971, 1269790.86314669973216951 6098820.61295877024531364, 1269797.6536356380674988 6098847.97393942717462778, 1269806.36995176738128066 6098891.71842609904706478, 1269815.37569857249036431 6098950.24989991821348667, 1269827.26462018908932805 6099009.23087266180664301, 1269838.89750697719864547 6099058.23229711316525936, 1269850.76416469551622868 6099101.49550071079283953, 1269864.70136494282633066 6099146.8381030410528183, 1269879.62930865841917694 6099181.56877079606056213, 1269911.15498845116235316 6099249.99924759287387133, 1269934.25378279038704932 6099304.37475735601037741, 1269966.2804002920165658 6099388.45871089026331902, 1270001.89150539645925164 6099486.49949030112475157, 1270015.80644174572080374 6099525.17384527251124382, 1270029.81043368740938604 6099569.95314655639231205, 1270037.63619389035739005 6099602.15712890867143869, 1270047.18740620044991374 6099641.06489250250160694, 1270054.7571315742097795 6099680.12254473380744457, 1270061.5253566144965589 6099726.00054469797760248, 1270091.73746641585603356 6099976.80355287436395884)";

            var wktReader = new WKTReader();
            var c = (wktReader.Read(point) as IPoint).ToCoordinate2D();
            var ab = wktReader.Read(line) as ILineString;

            var l = Spatial.Length(ab);
            var f = Spatial.Intercept(ab, c);
            var p = Spatial.Interpolate(ab, l, f);
            var d = Spatial.Distance(p, c);

            var expectedDistance = 1704.0;
            AssertEquals(l, expectedDistance, 1);
            var expectedP = new Coordinate2D(1269882.4308033068664372, 6099187.64979622885584831);
            AssertEquals(p.X, expectedP.X, 10);
            AssertEquals(p.Y, expectedP.Y, 10);
            AssertEquals(f, 0.5175157549609246, 10E-6);
            AssertEquals(d, 252.03375312704165, 10E-6);

        }


        [Fact]
        public void TestPathInterception2()
        {
            var point = "POINT(11.584009286555187 48.17578656762985)";
            var line = "LINESTRING(11.5852021 48.1761996, 11.585284 48.175924, 11.5852937 48.1758945)";

            var wktReader = new WKTReader();
            var c_4326 = wktReader.Read(point) as IPoint;
            var ab_4326 = wktReader.Read(line) as ILineString;

            /*
            var f = spatial.Intercept(ab, c);
            var l = spatial.Length(ab);
            var p = spatial.Interpolate(ab, l, f);
            var d = spatial.Distance(p, c);
            */

            /*
            assertEquals(p.getX(), 11.585274842230357, 10E-6);
            assertEquals(p.getY(), 48.17595481677191, 10E-6);
            assertEquals(f, 0.801975106391962, 10E-6);
            assertEquals(l, 34.603061318901396, 10E-6);
            assertEquals(d, 95.96239015496631, 10E-6);
            */
        }

        [Fact]
        public void TestLineAzimuth()
        {
            Assert.Equal(AzimuthEpsg4326(_berl_4326, _mosk_4326, true), Spatial.Azimuth(_berl_3395, _mosk_3395, 0f), 1);
            Assert.Equal(AzimuthEpsg4326(_berl_4326, _mosk_4326, false), Spatial.Azimuth(_berl_3395, _mosk_3395, 1f), 1);
            Assert.Equal(AzimuthEpsg4326(_berl_4326, _reyk_4326, true), Spatial.Azimuth(_berl_3395, _reyk_3395, 0f), 1);
            Assert.True(Spatial.Azimuth(_berl_4326, _mosk_4326, 0f) < Spatial.Azimuth(_berl_3395, _mosk_3395, 0.5)
                    && Spatial.Azimuth(_berl_4326, _mosk_4326, 0.5) < Spatial.Azimuth(_berl_3395, _mosk_3395, 1f));
        }

        [Fact]
        public void TestPathAzimuth()
        {
            var reykEpsg4326 = new Coordinate2D(-21.933333, 64.15);
            var berlEpsg4326 = new Coordinate2D(13.408056, 52.518611);
            var moskEpsg4326 = new Coordinate2D(37.616667, 55.75);

            var reykEpsg3395 = _transformation4326To3395.Transform(reykEpsg4326);
            var berlEpsg3395 = _transformation4326To3395.Transform(berlEpsg4326);
            var moskEpsg3395 = _transformation4326To3395.Transform(moskEpsg4326);

            var p = new LineString(new Coordinate[] {
                berlEpsg3395.ToGeoAPICoordinate(),
                moskEpsg3395.ToGeoAPICoordinate(),
                reykEpsg3395.ToGeoAPICoordinate()
            });

            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, moskEpsg4326, true), Spatial.Azimuth(p, 0), 1);
            Assert.Equal(AzimuthEpsg4326(moskEpsg4326, reykEpsg3395, false), Spatial.Azimuth(p, 1), 1);
            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, moskEpsg4326, false),
                    Spatial.Azimuth(p, Spatial.Distance(berlEpsg3395, moskEpsg3395) / Spatial.Length(p)), 1);

            var c = Spatial.Interpolate(berlEpsg3395, moskEpsg3395, 0.5);
            var c4326 = _transformation3395To4326.Transform(c);
            Assert.Equal(AzimuthEpsg4326(berlEpsg4326, c4326, false),
                    Spatial.Azimuth(p, Spatial.Distance(berlEpsg3395, c) / Spatial.Length(p)), 1);

            var d = Spatial.Interpolate(moskEpsg3395, reykEpsg3395, 0.5);
            var d4326 = _transformation3395To4326.Transform(d);
            Assert.Equal(AzimuthEpsg4326(moskEpsg4326, d4326, false), Spatial.Azimuth(p,
                    (Spatial.Distance(berlEpsg3395, moskEpsg3395) + Spatial.Distance(moskEpsg3395, d)) / Spatial.Length(p)), 1);
        }

        private static double AzimuthEpsg4326(in Coordinate2D a, in Coordinate2D b, bool left)
        {
            var geod = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X);
            var azi = left ? geod.azi1 : geod.azi2;
            return azi < 0 ? azi + 360 : azi;
        }

        private double DistanceEpsg4326(Coordinate2D a, Coordinate2D b)
        {
            return Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X).s12;
        }

    }
#endif

}
