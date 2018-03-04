using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Sandwych.MapMatchingKit.Topology;

namespace Sandwych.MapMatchingKit.Tests.Topology
{
    public abstract class AbstractRouterTest : TestBase
    {
        protected abstract IGraphRouter<Road, RoadPoint> CreateRouter(Graph graph, Func<Road, double> cost, Func<Road, double> bound, double max);

        protected void AssertSinglePath(
            IEnumerable<long> expectedPath,
            IEnumerable<RoadPoint> sources,
            IEnumerable<RoadPoint> targets,
            IReadOnlyDictionary<RoadPoint, (RoadPoint, IEnumerable<Road>)> routes)
        {
            Assert.Equal(1, routes.Count);
            var route = routes[targets.First()];

            Assert.Equal(expectedPath.First(), route.Item1.Edge.Id);
            Assert.Equal(expectedPath, route.Item2.Select(r => r.Id));
        }

        protected void AssertMultiplePathsTest(
            IReadOnlyDictionary<RoadPoint, (RoadPoint, IEnumerable<Road>)> routes,
            IReadOnlyDictionary<long, IEnumerable<long>> expectedPaths,
            IEnumerable<RoadPoint> sources, IEnumerable<RoadPoint> targets)
        {
            Assert.Equal(expectedPaths.Count(), routes.Count());

            foreach (var pair in routes)
            {
                var route = pair.Value.Item2;
                var expectedPath = expectedPaths[pair.Key.Edge.Id];

                Assert.NotNull(route);
                Assert.Equal(expectedPath.First(), pair.Value.Item1.Edge.Id);
                Assert.Equal(expectedPath.Count(), route.Count());
                Assert.Equal(expectedPath, route.Select(r => r.Id));
            }
        }

        [Theory]
        [ClassData(typeof(RouterTestData_SameRoad))]
        public void TestSameRoad(
            Graph graph,
            IEnumerable<long> expectedPath, IEnumerable<RoadPoint> sources, IEnumerable<RoadPoint> targets,
            Func<Road, double> cost, Func<Road, double> bound, double max)
        {
            var route = this.CreateRouter(graph, cost, bound, max);
            var routes = route.Route(sources, targets, cost, bound, max);
            this.AssertSinglePath(expectedPath, sources, targets, routes);
        }

        [Theory]
        [ClassData(typeof(RouterTestData_SelfLoop))]
        public void TestSelfLoop(
            Graph graph,
            IEnumerable<long> expectedPath, IEnumerable<RoadPoint> sources, IEnumerable<RoadPoint> targets,
            Func<Road, double> cost, Func<Road, double> bound, double max)
        {
            var route = this.CreateRouter(graph, cost, bound, max);
            var routes = route.Route(sources, targets, cost, bound, max);
            this.AssertSinglePath(expectedPath, sources, targets, routes);
        }


        /*
        [Fact]
        public void TestShortestPath()
        {
            var roads = new Road[] {
                new Road(0, 0, 1, 100),
                new Road(1, 1, 0, 100),
                new Road(2, 0, 2, 160),
                new Road(3, 2, 0, 160),
                new Road(4, 1, 2, 50),
                new Road(5, 2, 1, 50),
                new Road(6, 1, 3, 200),
                new Road(7, 3, 1, 200),
                new Road(8, 2, 3, 100),
                new Road(9, 3, 2, 100),
                new Road(10, 2, 4, 40),
                new Road(11, 4, 2, 40),
                new Road(12, 3, 4, 100),
                new Road(13, 4, 3, 100),
                new Road(14, 3, 5, 200),
                new Road(15, 5, 3, 200),
                new Road(16, 4, 5, 60),
                new Road(17, 5, 4, 60),
            };
            var map = new Graph(roads);
            {
                // (0.7, 100) + 50 + 40 + 60 + (0.3, 200) = 280
                var router = this.CreateRouter();
                var paths = new Dictionary<long, IEnumerable<long>>() {
                    { 14L, new long[] { 0L, 4L, 8L, 14L } },
                    { 15L, new long[] { 0L, 4L, 10L, 16L, 15L } },
                };
                AssertMultiplePaths(
                    router,
                    expectedPaths: paths,
                    sources: new RoadPoint[] { new RoadPoint(map.EdgeMap[0], 0.3), new RoadPoint(map.EdgeMap[1], 0.7) },
                    targets: new RoadPoint[] { new RoadPoint(map.EdgeMap[14], 0.3), new RoadPoint(map.EdgeMap[15], 0.7) });
            }
            {
                // (0.7, 100) + 50 + 100 + (0.1, 200) = 240
                var router = this.CreateRouter();
                var paths = new Dictionary<long, IEnumerable<long>>() {
                    { 14L, new long[] { 0L, 4L, 8L, 14L } },
                    { 15L, new long[] { 0L, 4L, 10L, 16L, 15L } },
                };
                AssertMultiplePaths(
                    router,
                    expectedPaths: paths,
                    sources: new RoadPoint[] { new RoadPoint(map.EdgeMap[0], 0.3), new RoadPoint(map.EdgeMap[1], 0.7) },
                    targets: new RoadPoint[] { new RoadPoint(map.EdgeMap[14], 0.1), new RoadPoint(map.EdgeMap[15], 0.9) });
            }

            {
                var router = CreateRouter();
                // (0.7, 100) + 50 + 100 + (0.1, 200) = 240
                var sources = new RoadPoint[] { new RoadPoint(map.GetEdge(0), 0.3), };
                var targets = new RoadPoint[] { new RoadPoint(map.GetEdge(14), 0.1), };
                var route = router.Route(sources, targets, e => e.Weight, e => e.Weight, 200.0);

                Assert.NotNull(route);
                Assert.Empty(route);
            }
            {
                var router = CreateRouter();
                // (0.7, 100) + 50 + 100 + (0.1, 200) = 240
                // (0.7, 100) + 50 + 100 + (0.8, 200) = 380

                var sources = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3),
                    new RoadPoint(map.GetEdge(1), 0.7),
                };
                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(14), 0.1),
                    new RoadPoint(map.GetEdge(14), 0.8),
                };
                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);
                var paths = new Dictionary<long, long[]>() {
                    { 14L, new long[] { 0L, 4L, 8L, 14L } }
                };

                Assert.Equal(2, routes.Count());

                foreach (var pair in routes)
                {
                    var route = pair.Value.Item2;
                    var path = paths[pair.Key.Edge.Id];

                    Assert.NotNull(route);
                    Assert.Equal(path[0], pair.Value.Item1.Edge.Id);
                    Assert.Equal(path.Count(), route.Count());

                    int i = 0;
                    foreach (var road in route)
                    {
                        Assert.Equal((long)path[i++], road.Id);
                    }
                }
            }
        }
        */

    }
}
