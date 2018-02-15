using GeoAPI.Geometries;
using GeographicLib;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Spatial.Projection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Spatial
{

    public class GeographySpatialOperationTest : AbstractSpatialOperationTest
    {
        protected override ISpatialOperation Spatial => new GeographySpatialOperation();

        private double Distance(in Coordinate2D a, in Coordinate2D b)
        {
            return this.Spatial.Distance(a, b);
        }

        [Fact]
        public void TestDistance()
        {
            var reyk = new Coordinate2D(-21.933333, 64.15);
            var berl = new Coordinate2D(13.408056, 52.518611);
            var mosk = new Coordinate2D(37.616667, 55.75);

            var dist_geog = Spatial.Distance(mosk, reyk);
            var dist_esri = Distance(mosk, reyk);

            AssertEquals(dist_geog, dist_esri, 10E-6);
            dist_geog = Spatial.Distance(berl, reyk);
            dist_esri = Distance(berl, reyk);
            AssertEquals(dist_geog, dist_esri, 10E-6);
        }

        [Fact]
        public void TestGnomonic()
        {
            var reyk = new Coordinate2D(-21.933333, 64.15);
            var berl = new Coordinate2D(13.408056, 52.518611);
            var mosk = new Coordinate2D(37.616667, 55.75);

            var f = Spatial.Intercept(reyk, mosk, berl);
            var p = Spatial.Interpolate(reyk, mosk, f);
            var res = Intercept(reyk, mosk, berl);

            AssertEquals(f, res.Item3, 0.1);
            AssertEquals(p.X, res.Item1.X, 10E-2);
            AssertEquals(p.Y, res.Item1.Y, 10E-2);
        }

        [Fact]
        public void TestLineInterception()
        {
            var wktReader = new WKTReader();
            var ab = wktReader.Read("LINESTRING(11.4047661 48.1403687,11.4053519 48.141055)") as ILineString;
            var a = ab.GetPointN(0).ToCoordinate2D();
            var b = ab.GetPointN(1).ToCoordinate2D();

            var points = new string[] {
                "POINT(11.406501117689324 48.14051652560591)", // East
                "POINT(11.406713245538327 48.14182906667162)", // Northeast
                "POINT(11.404923416812364 48.14258477213369)", // North
                "POINT(11.403300759321036 48.14105540093837)", // Northwest
                "POINT(11.403193249043934 48.140881120346386)", // West
                "POINT(11.40327279698731 48.13987351306362)", // Southwest
                "POINT(11.405221721600025 48.1392039845402)", // South
                "POINT(11.406255844863914 48.13963486923349)" // Southeast
            };

            for (int i = 0; i < points.Length; ++i)
            {
                var c = (wktReader.Read(points[i]) as IPoint).ToCoordinate2D();

                var f = Spatial.Intercept(a, b, c);
                var p = Spatial.Interpolate(a, b, f);
                var res = Intercept(a, b, c);
                var s = Spatial.Distance(p, c);
                var s_esri = Distance(p, c);

                AssertEquals(f > 1 ? 1 : f < 0 ? 0 : f, res.Item3, 0.2);
                AssertEquals(p.X, res.Item1.X, 10E-2);
                AssertEquals(p.Y, res.Item1.Y, 10E-2);
                AssertEquals(s, s_esri, 10E-6);
            }
        }


        private static double Azimuth(Coordinate2D a, Coordinate2D b, bool left)
        {
            var geod = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X);
            var azi = left ? geod.azi1 : geod.azi2;
            return azi < 0 ? azi + 360 : azi;
        }

        [Fact]
        public void TestLineAzimuth()
        {
            var reyk = new Coordinate2D(-21.933333, 64.15);
            var berl = new Coordinate2D(13.408056, 52.518611);
            var mosk = new Coordinate2D(37.616667, 55.75);

            AssertEquals(Azimuth(berl, mosk, true), Spatial.Azimuth(berl, mosk, 0f), 1E-9);
            AssertEquals(Azimuth(berl, mosk, false), Spatial.Azimuth(berl, mosk, 1f), 1E-9);
            AssertEquals(Azimuth(berl, reyk, true), Spatial.Azimuth(berl, reyk, 0f), 1E-9);
            Assert.True(Spatial.Azimuth(berl, mosk, 0f) < Spatial.Azimuth(berl, mosk, 0.5)
                    && Spatial.Azimuth(berl, mosk, 0.5) < Spatial.Azimuth(berl, mosk, 1f));
        }

        [Fact]
        public void TestPathInterception1()
        {
            var wktReader = new WKTReader();
            var point = "POINT(11.410624 48.144161)";
            var line =
                    "LINESTRING(11.4047013 48.1402147,11.4047038 48.1402718,11.4047661 48.1403687,11.4053519 48.141055,11.4054617 48.1411901,11.4062664 48.1421968,11.4064586 48.1424479,11.4066449 48.1427372,11.4067254 48.1429028,11.4067864 48.1430673,11.4068647 48.1433303,11.4069456 48.1436822,11.4070524 48.1440368,11.4071569 48.1443314,11.4072635 48.1445915,11.4073887 48.1448641,11.4075228 48.1450729,11.407806 48.1454843,11.4080135 48.1458112,11.4083012 48.1463167,11.4086211 48.1469061,11.4087461 48.1471386,11.4088719 48.1474078,11.4089422 48.1476014,11.409028 48.1478353,11.409096 48.1480701,11.4091568 48.1483459,11.4094282 48.1498536)";

            var c = (wktReader.Read(point) as IPoint).ToCoordinate2D();
            var ab = wktReader.Read(line) as ILineString;

            var f = Spatial.Intercept(ab, c);
            var l = Spatial.Length(ab);
            var p = Spatial.Interpolate(ab, l, f);
            var d = Spatial.Distance(p, c);
            AssertEquals(p.X, 11.407547966254612, 10E-6);
            AssertEquals(p.Y, 48.14510945890138, 10E-6);
            AssertEquals(f, 0.5175157549609246, 10E-6);
            AssertEquals(l, 1138.85464239099, 10E-6);
            AssertEquals(d, 252.03375312704165, 10E-6);

        }

        [Fact]
        public void TestPathInterception2()
        {
            var wktReader = new WKTReader();
            String point = "POINT(11.584009286555187 48.17578656762985)";
            String line = "LINESTRING(11.5852021 48.1761996, 11.585284 48.175924, 11.5852937 48.1758945)";

            var c = (wktReader.Read(point) as IPoint).ToCoordinate2D();
            var ab = wktReader.Read(line) as ILineString;

            var f = Spatial.Intercept(ab, c);
            var l = Spatial.Length(ab);
            var p = Spatial.Interpolate(ab, l, f);
            var d = Spatial.Distance(p, c);

            AssertEquals(p.X, 11.585274842230357, 10E-6);
            AssertEquals(p.Y, 48.17595481677191, 10E-6);
            AssertEquals(f, 0.801975106391962, 10E-6);
            AssertEquals(l, 34.603061318901396, 10E-6);
            AssertEquals(d, 95.96239015496631, 10E-6);
        }

        [Fact]
        public void TestPathAzimuth()
        {
            var reyk = new Coordinate2D(-21.933333, 64.15);
            var berl = new Coordinate2D(13.408056, 52.518611);
            var mosk = new Coordinate2D(37.616667, 55.75);

            var p = new LineString(new Coordinate[] {
                berl.ToGeoAPICoordinate(),
                mosk.ToGeoAPICoordinate(),
                reyk.ToGeoAPICoordinate()
            });

            AssertEquals(Azimuth(berl, mosk, true), Spatial.Azimuth(p, 0f), 1E-9);
            AssertEquals(Azimuth(mosk, reyk, false), Spatial.Azimuth(p, 1f), 1E-9);
            AssertEquals(Azimuth(berl, mosk, false),
                    Spatial.Azimuth(p, Spatial.Distance(berl, mosk) / Spatial.Length(p)), 1E-9);

            var c = Spatial.Interpolate(berl, mosk, 0.5);
            AssertEquals(Azimuth(berl, c, false),
                    Spatial.Azimuth(p, Spatial.Distance(berl, c) / Spatial.Length(p)), 1E-9);

            var d = Spatial.Interpolate(mosk, reyk, 0.5);
            AssertEquals(Azimuth(mosk, d, false), Spatial.Azimuth(p,
                    (Spatial.Distance(berl, mosk) + Spatial.Distance(mosk, d)) / Spatial.Length(p)),
                    1E-9);
        }

    }

}
