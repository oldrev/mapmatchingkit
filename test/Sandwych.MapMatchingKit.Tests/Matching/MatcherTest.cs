using GeoAPI.Geometries;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Matching;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Matching
{
    public class MatcherTest : TestBase
    {
        private readonly ISpatialOperation _spatial = new GeographySpatialOperation();
        private readonly DijkstraRouter<Road, RoadPoint> _router = new DijkstraRouter<Road, RoadPoint>();
        private readonly RoadMap _map;
        private readonly Func<Road, double> _cost = new Func<Road, double>(Costs.TimeCost);

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

            public MockedRoadReader(ISpatialOperation spatial)
            {
                var wktRdr = new WKTReader();
                foreach (var e in _entries)
                {
                    var geom = wktRdr.Read("SRID=4326;" + e.Item5) as ILineString;
                    _roads.Add(new RoadInfo(e.Item1, e.Item2, e.Item3, e.Item4, (short)0, 1.0f, 100f, 100f, (float)spatial.Length(geom), geom));
                }
                _enumerator = _roads.GetEnumerator();
            }
        }

        public MatcherTest()
        {
            var reader = new MockedRoadReader(_spatial);
            var roadMapBuilder = new RoadMapBuilder(_spatial);
            _map = roadMapBuilder.AddRoads(reader.Roads).Build();
        }

        private void AssertCandidate(in CandidateProbability<MatcherCandidate> candidate, Coordinate2D sample)
        {
            var polyline = _map.GetEdge(candidate.Candidate.Point.Edge.Id).Geometry;
            var f = _spatial.Intercept(polyline, sample);
            var i = _spatial.Interpolate(polyline, f);
            var l = _spatial.Distance(i, sample);
            var sig2 = Math.Pow(5d, 2);
            var sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * sig2);
            var p = 1 / sqrt_2pi_sig2 * Math.Exp((-1) * l * l / (2 * sig2));

            AssertEquals(f, candidate.Candidate.Point.Fraction, 10E-6);
            AssertEquals(p, candidate.Probability, 10E-6);
        }

        private void AssertTransition(in TransitionProbability<MatcherTransition> transition,
                in (MatcherCandidate, MatcherSample) source,
                in (MatcherCandidate, MatcherSample) target, double lambda)
        {
            var edges = _router.Route(source.Item1.Point, target.Item1.Point, _cost);
            Assert.NotNull(edges);

            var route = new Route(source.Item1.Point, target.Item1.Point, edges);

            AssertEquals(route.Length, transition.Transition.Route.Length, 10E-6);
            Assert.Equal(route.StartPoint.Edge.Id, transition.Transition.Route.StartPoint.Edge.Id);
            Assert.Equal(route.EndPoint.Edge.Id, transition.Transition.Route.EndPoint.Edge.Id);

            double beta = lambda == 0 ? (2.0 * (target.Item2.Time - source.Item2.Time).TotalSeconds)
                    : 1 / lambda;
            double @base = 1.0 * _spatial.Distance(source.Item2.Coordinate, target.Item2.Coordinate) / 60;
            double p = (1 / beta)
                    * Math.Exp((-1.0) * Math.Max(0, route.Cost(Costs.TimePriorityCost) - @base) / beta);

            AssertEquals(transition.Probability, p, 10E-6);
        }

        private ISet<long> RefSet(Coordinate2D sample, double radius)
        {
            var refset = new HashSet<long>();
            foreach (var road in _map.EdgeMap.Values)
            {
                double f = _spatial.Intercept(road.Geometry, sample);
                var i = _spatial.Interpolate(road.Geometry, f);
                double l = _spatial.Distance(i, sample);

                if (l <= radius)
                {
                    refset.Add(road.Id);
                }
            }
            return refset;
        }

        [Fact]
        public void TestCandidates()
        {
            var filter = new Matcher(_map, _router, _cost, _spatial);
            {
                filter.MaxRadius = 100D;
                var sample = new Coordinate2D(11.001, 48.001);

                var candidates = filter.ComputeCandidates(new MatcherCandidate[] { }, new MatcherSample(0, 0, sample));

                Assert.Empty(candidates);
            }

            void assertCandidate(double radius, Coordinate2D sample, IEnumerable<long> refsetIds)
            {
                filter.MaxRadius = radius;

                var candidates = filter.ComputeCandidates(new MatcherCandidate[] { }, new MatcherSample(0, 0, sample));

                var refset = new HashSet<long>(refsetIds);
                var set = new HashSet<long>();

                foreach (var candidate in candidates)
                {
                    Assert.Contains(candidate.Candidate.Point.Edge.Id, refset);
                    AssertCandidate(candidate, sample);
                    set.Add(candidate.Candidate.Point.Edge.Id);
                }

                Assert.Equal(refset, set);
            }

            assertCandidate(200D, new Coordinate2D(11.001, 48.001), new long[] { 0L, 1L });
            assertCandidate(200D, new Coordinate2D(11.010, 48.000), new long[] { 0L, 3L });
            assertCandidate(200D, new Coordinate2D(11.011, 48.001), new long[] { 0L, 2L, 3L });
            assertCandidate(300D, new Coordinate2D(11.011, 48.001), new long[] { 0L, 2L, 3L, 8L });
            assertCandidate(300D, new Coordinate2D(11.011, 48.001), new long[] { 0L, 2L, 3L, 8L });
            assertCandidate(200D, new Coordinate2D(11.019, 48.001), new long[] { 2L, 3L, 5L, 10L });
        }

        [Fact]
        public void TestTransitions()
        {
            var filter = new Matcher(_map, _router, _cost, _spatial);
            filter.MaxRadius = 200D;
            {
                MatcherSample sample1 = new MatcherSample(0, 0, new Coordinate2D(11.001, 48.001));
                MatcherSample sample2 = new MatcherSample(1, 60000, new Coordinate2D(11.019, 48.001));

                var predecessors = new HashSet<MatcherCandidate>();
                var candidates = new HashSet<MatcherCandidate>();

                foreach (var candidate in filter.ComputeCandidates(new HashSet<MatcherCandidate>(), sample1))
                {
                    predecessors.Add(candidate.Candidate);
                }

                foreach (var candidate in filter.ComputeCandidates(new HashSet<MatcherCandidate>(), sample2))
                {
                    candidates.Add(candidate.Candidate);
                }

                Assert.Equal(2, predecessors.Count);
                Assert.Equal(4, candidates.Count);

                var transitions = filter.ComputeTransitions((sample1, predecessors), (sample2, candidates));

                Assert.Equal(2, transitions.Count);

                foreach (var source in transitions)
                {
                    Assert.Equal(4, source.Value.Count);

                    foreach (var target in source.Value)
                    {
                        AssertTransition(target.Value, (source.Key, sample1), (target.Key, sample2), filter.Lambda);
                    }

                }
            }
            {
                var sample1 = new MatcherSample(0, 0, new Coordinate2D(11.019, 48.001));
                var sample2 = new MatcherSample(1, 60000, new Coordinate2D(11.001, 48.001));

                var predecessors = new HashSet<MatcherCandidate>();
                var candidates = new HashSet<MatcherCandidate>();

                foreach (var candidate in filter.ComputeCandidates(new HashSet<MatcherCandidate>(), sample1))
                {
                    predecessors.Add(candidate.Candidate);
                }

                foreach (var candidate in filter.ComputeCandidates(new HashSet<MatcherCandidate>(), sample2))
                {
                    candidates.Add(candidate.Candidate);
                }

                Assert.Equal(4, predecessors.Count);
                Assert.Equal(2, candidates.Count);

                var transitions = filter.ComputeTransitions((sample1, predecessors), (sample2, candidates));

                Assert.Equal(4, transitions.Count);

                foreach (var source in transitions)
                {
                    if (source.Key.Point.Edge.Id == 10)
                    {
                        Assert.Equal(0, source.Value.Count);
                    }
                    else
                    {
                        Assert.Equal(2, source.Value.Count);
                    }

                    foreach (var target in source.Value)
                    {
                        AssertTransition(target.Value, (source.Key, sample1), (target.Key, sample2), filter.Lambda);
                    }
                }
            }
        } //end function TestTransitions

    } //end class
} //end namespace
