using GeoAPI.Geometries;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Matcher
{
    public class MatcherTest : TestBase
    {
        private readonly static ISpatialOperation s_spatial = new GeographySpatialOperation();
        private readonly DijkstraRouter<Road, RoadPoint> _router = new DijkstraRouter<Road, RoadPoint>();

        class MockedRoadReader
        {
            private readonly List<RoadInfo> _roads = new List<RoadInfo>();
            private readonly (long, long, long, bool, string)[] _entries = new(long, long, long, bool, string)[]
            {
                (0L, 0L, 1L, false, "LINESTRING(11.000 48.000, 11.010 48.000)"),
                (1L, 1L, 2L, false, "LINESTRING(11.010 48.000, 11.020 48.000)"),
                (2L, 2L, 3L, false, "LINESTRING(11.020 48.000, 11.030 48.000)"),
                (3L, 1L, 4L, true, "LINESTRING(11.010 48.000, 11.011 47.999)"),
                (4L, 4L, 5L, true, "LINESTRING(11.011 47.999, 11.021 47.999)"),
                (5L, 5L, 6L, true, "LINESTRING(11.021 47.999, 11.021 48.010)")
            };
            private IEnumerator<RoadInfo> _enumerator;

            public IEnumerable<RoadInfo> Roads => _roads;

            public MockedRoadReader()
            {
                var wktRdr = new WKTReader();
                foreach (var e in _entries)
                {
                    var geom = wktRdr.Read(e.Item5) as ILineString;
                    _roads.Add(new RoadInfo(e.Item1, e.Item2, e.Item3, e.Item1, e.Item4, (short)0, 1.0f, 100f, 100f, (float)s_spatial.Length(geom), geom));
                }
                _enumerator = _roads.GetEnumerator();
            }
        }

        public MatcherTest()
        {

        }

    }
}
