using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Matching;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Matching
{

    public class MinsetTest : TestBase
    {
        class MockedRoadReader1
        {
            private readonly List<RoadInfo> _roads = new List<RoadInfo>();
            private readonly (long, long, long, bool, string)[] _entries = new(long, long, long, bool, string)[]
            {
                (0L, 0L, 2L, true, "LINESTRING(0 0, 1 1)"),
                (1L, 1L, 2L, true, "LINESTRING(0 2, 1 1)"),
                (2L, 2L, 3L, true, "LINESTRING(1 1, 2 1)"),
                (3L, 3L, 4L, true, "LINESTRING(2 1, 3 2)"),
                (4L, 3L, 5L, true, "LINESTRING(2 1, 3 1)"),
                (5L, 3L, 6L, true, "LINESTRING(2 1, 3 0)")
            };
            private IEnumerator<RoadInfo> _enumerator;

            public IEnumerable<RoadInfo> Roads => _roads;

            public MockedRoadReader1()
            {
                var wktRdr = new WKTReader();
                foreach (var e in _entries)
                {
                    var geom = wktRdr.Read(e.Item5) as ILineString;
                    _roads.Add(new RoadInfo(e.Item1, e.Item2, e.Item3, e.Item1, e.Item4, (short)0, 0f, 0f, 0f, 0f, geom));
                }
                _enumerator = _roads.GetEnumerator();
            }
        }

        class MockedRoadReader2
        {
            private readonly List<RoadInfo> _roads = new List<RoadInfo>();
            private readonly (long, long, long, bool, string)[] _entries = new(long, long, long, bool, string)[] {
                    (0L, 0L, 1L, false, "LINESTRING(11.000 48.000, 11.010 48.000)"),
                    (1L, 1L, 2L, false, "LINESTRING(11.010 48.000, 11.020 48.000)"),
                    (2L, 2L, 3L, false, "LINESTRING(11.020 48.000, 11.030 48.000)"),
                    (3L, 1L, 4L, true, "LINESTRING(11.010 48.000, 11.011 47.999)"),
                    (4L, 4L, 5L, true, "LINESTRING(11.011 47.999, 11.021 47.999)"),
                    (5L, 5L, 6L, true, "LINESTRING(11.021 47.999, 11.021 48.010)")
            };

            private IEnumerator<RoadInfo> _enumerator;

            public IEnumerable<RoadInfo> Roads => _roads;

            public MockedRoadReader2()
            {
                var wktRdr = new WKTReader();
                foreach (var e in _entries)
                {
                    var geom = wktRdr.Read(e.Item5) as ILineString;
                    var spatial = new GeographySpatialOperation();
                    _roads.Add(new RoadInfo(e.Item1, e.Item2, e.Item3, e.Item1, e.Item4, (short)0, 1.0f, 100.0f, 100.0f, (float)spatial.Length(geom), geom));
                }
                _enumerator = _roads.GetEnumerator();
            }
        }


        [Fact]
        public void TestMinset1()
        {
            var reader = new MockedRoadReader1();
            var roadMapBuilder = new RoadMapBuilder();
            var map = roadMapBuilder.AddRoads(reader.Roads).Build();
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(0), 1),
                    new RoadPoint(map.GetEdge(2), 1),
                    new RoadPoint(map.GetEdge(4), 0.5),
                    new RoadPoint(map.GetEdge(6), 0),
                    new RoadPoint(map.GetEdge(8), 0),
                    new RoadPoint(map.GetEdge(10), 0)
                };

                var minset = Minset.Minimize(candidates);

                Assert.Equal(1, minset.Count);
                Assert.Equal(4, minset.First().Road.Id);
            }
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(0), 1),
                    new RoadPoint(map.GetEdge(2), 1),
                    new RoadPoint(map.GetEdge(4), 1),
                    new RoadPoint(map.GetEdge(8), 0.5),
                    new RoadPoint(map.GetEdge(10), 0.5)
                };

                var minset = Minset.Minimize(candidates);

                var refset = new HashSet<long>() { 4L, 8L, 10L };
                var set = new HashSet<long>();
                foreach (RoadPoint element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(4), 1),
                    new RoadPoint(map.GetEdge(6), 0.0),
                    new RoadPoint(map.GetEdge(8), 0.5),
                    new RoadPoint(map.GetEdge(10), 0.5),
                };

                var minset = Minset.Minimize(candidates);

                var refset = new HashSet<long>() { 4L, 8L, 10L };
                var set = new HashSet<long>();
                foreach (var element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
            {
                var candidates = new HashSet<RoadPoint>() {
                        new RoadPoint(map.GetEdge(0), 1),
                        new RoadPoint(map.GetEdge(2), 1),
                        new RoadPoint(map.GetEdge(4), 1),
                        new RoadPoint(map.GetEdge(6), 0.2),
                        new RoadPoint(map.GetEdge(8), 0.5),
                        new RoadPoint(map.GetEdge(10), 0.5),
                    };

                var minset = Minset.Minimize(candidates);

                var refset = new HashSet<long>() { 6L, 8L, 10L };
                var set = new HashSet<long>();
                foreach (var element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
        }

        [Fact]
        public void TestMinset2()
        {
            var reader = new MockedRoadReader2();
            var roadMapBuilder = new RoadMapBuilder();
            var map = roadMapBuilder.AddRoads(reader.Roads).Build();
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(0), 1),
                    new RoadPoint(map.GetEdge(1), 0),
                    new RoadPoint(map.GetEdge(2), 0),
                    new RoadPoint(map.GetEdge(3), 1),
                    new RoadPoint(map.GetEdge(6), 0),
                    new RoadPoint(map.GetEdge(8), 0)
                };

                var minset = Minset.Minimize(candidates);
                var refset = new HashSet<long>() { 0L, 3L };
                var set = new HashSet<long>();
                foreach (RoadPoint element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(0), 1),
                    new RoadPoint(map.GetEdge(1), 0),
                    new RoadPoint(map.GetEdge(2), 0.1),
                    new RoadPoint(map.GetEdge(3), 0.9),
                    new RoadPoint(map.GetEdge(6), 0),
                    new RoadPoint(map.GetEdge(8), 0),
                };

                var minset = Minset.Minimize(candidates);

                var refset = new HashSet<long>() { 0L, 2L, 3L };
                var set = new HashSet<long>();
                foreach (RoadPoint element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
            {
                var candidates = new HashSet<RoadPoint>() {
                    new RoadPoint(map.GetEdge(0), 1),
                    new RoadPoint(map.GetEdge(1), 0),
                    new RoadPoint(map.GetEdge(2), 0.1),
                    new RoadPoint(map.GetEdge(3), 0.9),
                    new RoadPoint(map.GetEdge(6), 0),
                    new RoadPoint(map.GetEdge(8), 0.1),
                };

                var minset = Minset.Minimize(candidates);

                var refset = new HashSet<long>() { 0L, 2L, 3L, 8L };
                var set = new HashSet<long>();
                foreach (RoadPoint element in minset)
                {
                    Assert.Contains(element.Road.Id, refset);
                    set.Add(element.Road.Id);
                }

                Assert.Equal(refset, set);
            }
        }


    } //class

}