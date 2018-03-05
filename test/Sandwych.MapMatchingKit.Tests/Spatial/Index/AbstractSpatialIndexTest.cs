using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Spatial.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Spatial.Index
{
    public abstract class AbstractSpatialIndexTest : TestBase
    {
        private readonly IReadOnlyList<ILineString> _geometries;
        protected ISpatialOperation Spatial { get; }
        protected IReadOnlyList<ILineString> Geometries => _geometries;
        protected abstract ISpatialIndex<ILineString> CreateSpatialIndex();

        public AbstractSpatialIndexTest()
        {
            this.Spatial = new GeographySpatialOperation();
            _geometries = this.MakeGeometries();
        }

        private IReadOnlyList<ILineString> MakeGeometries()
        {
            /*
             * (p2) (p3) ----- (e1) : (p1) -> (p2) ----------------------------------------------------
             * - \ / --------- (e2) : (p3) -> (p1) ----------------------------------------------------
             * | (p1) | ------ (e3) : (p4) -> (p1) ----------------------------------------------------
             * - / \ --------- (e4) : (p1) -> (p5) ----------------------------------------------------
             * (p4) (p5) ----- (e5) : (p2) -> (p4) ----------------------------------------------------
             * --------------- (e6) : (p5) -> (p3) ----------------------------------------------------
             */
            String p1 = "11.3441505 48.0839963";
            String p2 = "11.3421209 48.0850624";
            String p3 = "11.3460348 48.0850108";
            String p4 = "11.3427522 48.0832129";
            String p5 = "11.3469701 48.0825356";
            var reader = new WKTReader();
            ILineString readAsLineString(string wkt) => reader.Read("SRID=4326;" + wkt) as ILineString;

            var geometries = new ILineString[] {
                readAsLineString("LINESTRING(" + p1 + "," + p2 + ")"),
                readAsLineString("LINESTRING(" + p3 + "," + p1 + ")"),
                readAsLineString("LINESTRING(" + p4 + "," + p1 + ")"),
                readAsLineString("LINESTRING(" + p1 + "," + p5 + ")"),
                readAsLineString("LINESTRING(" + p2 + "," + p4 + ")"),
                readAsLineString("LINESTRING(" + p5 + "," + p3 + ")"),
            };

            var geoms = new List<ILineString>();
            for (int i = 0; i < geometries.Length; i++)
            {
                var g = geometries[i];
                g.UserData = i;
                geoms.Add(g);
            }

            return geoms;
        }

        private void AssertIndexRadius(Coordinate2D c, double r, int expectedNeighborsCount)
        {
            var lines = this.Geometries;
            var index = this.CreateSpatialIndex();

            var neighbors = new HashSet<int>();
            for (int i = 0; i < lines.Count; ++i)
            {
                var line = lines[i];
                var f = this.Spatial.Intercept(line, c);
                var p = this.Spatial.Interpolate(line, f);
                var d = this.Spatial.Distance(c, p);

                if (d <= r)
                {
                    neighbors.Add(i);
                }
            }

            Assert.Equal(expectedNeighborsCount, neighbors.Count);

            var points = index.Radius(c, r);

            Assert.Equal(neighbors.Count, points.Count());
            foreach (var pointId in points.Select(p => (int)p.Item1.UserData))
            {
                Assert.Contains(pointId, neighbors);
            }
        }

        [Fact]
        public void TestIndexRadius()
        {
            {
                var c = new Coordinate2D(11.343629, 48.083797);
                var r = 50;
                var nc = 4;
                AssertIndexRadius(c, r, nc);
            }
            {
                var c = new Coordinate2D(11.344827, 48.083752);
                var r = 10;
                var nc = 1;
                AssertIndexRadius(c, r, nc);
            }
            {
                var c = new Coordinate2D(11.344827, 48.083752);
                var r = 5;
                var nc = 0;
                AssertIndexRadius(c, r, nc);
            }

        }

    }
}
