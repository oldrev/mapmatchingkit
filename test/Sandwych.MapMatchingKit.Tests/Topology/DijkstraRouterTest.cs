using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandwych.MapMatchingKit.Topology;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Topology
{
    public class DijkstraRouterTest
    {
        class Road : AbstractGraphEdge<Road>
        {
            public float Weight { get; }

            public Road(long id, long source, long target, float weight) : base(id, source, target)
            {
                this.Weight = weight;
            }

            public override int GetHashCode() =>
                (this.Id, this.Weight).GetHashCode();
        }

        class Graph : AbstractGraph<Road>
        {
            public Graph(IEnumerable<Road> roads) : base(roads)
            {

            }
        }

        class RoadPoint : IEdgePoint<Road>
        {
            public Road Edge { get; }
            public double Fraction { get; }

            public RoadPoint(Road road, double fraction)
            {
                this.Edge = road;
                this.Fraction = fraction;
            }

            public override int GetHashCode() =>
                (this.Edge, this.Fraction).GetHashCode();
        }

        [Fact]
        public void TestSameRoad()
        {
            var roads = new Road[] {
                new Road(0, 0, 1, 100),
                new Road(1, 1, 0, 20),
                new Road(2, 0, 2, 100),
                new Road(3, 1, 2, 100),
                new Road(4, 1, 3, 100)
            };
            var map = new Graph(roads);

            var router = new DijkstraRouter<Road, RoadPoint>();
            {
                var sources = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.3) };
                var targets = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.3) };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.3) };
                var targets = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.7) };
                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.7) };
                var targets = new RoadPoint[] { new RoadPoint(map.Edges[0L], 0.3) };
                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L, 1L, 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.8),
                    new RoadPoint(map.GetEdge(1), 0.2),
                };
                var targets = new RoadPoint[] { new RoadPoint(map.GetEdge(0), 0.7) };
                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 1L, 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
        }

        [Fact]
        public void TestSelfLoop()
        {
            var roads = new Road[] {
                new Road(0, 0, 0, 100),
                new Road(1, 0, 0, 100),
            };
            var map = new Graph(roads);
            var router = new DijkstraRouter<Road, RoadPoint>();
            {
                var sources = new RoadPoint[] { new RoadPoint(map.GetEdge(0), 0.3) };
                var targets = new RoadPoint[] { new RoadPoint(map.GetEdge(0), 0.7) };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] {
                    new RoadPoint( map.GetEdge(0), 0.7)
                };
                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3)
                };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L, 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] {
                    new RoadPoint( map.GetEdge(0), 0.8),
                    new RoadPoint( map.GetEdge(1), 0.2),
                };
                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.2)
                };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 0L, 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
            {
                var sources = new RoadPoint[] {
                    new RoadPoint( map.GetEdge(0), 0.4),
                    new RoadPoint( map.GetEdge(1), 0.6),
                };
                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3)
                };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var route = routes[targets.First()];
                var path = new long[] { 1L, 0L };

                Assert.Equal(path.First(), route.Item1.Edge.Id);
                Assert.Equal(path.Length, route.Item2.Count());

                int i = 0;
                foreach (var road in route.Item2)
                {
                    Assert.Equal(path[i++], road.Id);
                }
            }
        }

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
            var router = new DijkstraRouter<Road, RoadPoint>();
            {
                // (0.7, 100) + 50 + 40 + 60 + (0.3, 200) = 280

                var sources = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3),
                    new RoadPoint(map.GetEdge(1), 0.7),
                };

                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(14), 0.3),
                    new RoadPoint(map.GetEdge(15), 0.7),
                };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var paths = new Dictionary<long, long[]>() {
                    { 14L, new long[] { 0L, 4L, 8L, 14L } },
                    { 15L, new long[] { 0L, 4L, 10L, 16L, 15L } },
                };

                Assert.Equal(paths.Count(), routes.Count());

                foreach (var pair in routes)
                {
                    var route = pair.Value.Item2;
                    Assert.NotNull(paths[pair.Key.Edge.Id]);
                    var path = paths[pair.Key.Edge.Id];

                    Assert.NotNull(route);
                    Assert.Equal(path.First(), pair.Value.Item1.Edge.Id);
                    Assert.Equal(path.Count(), route.Count());

                    int i = 0;
                    foreach (var road in route)
                    {
                        Assert.Equal((long)path[i++], road.Id);
                    }
                }
            }
            {
                // (0.7, 100) + 50 + 100 + (0.1, 200) = 240

                var sources = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3),
                    new RoadPoint(map.GetEdge(1), 0.7),
                };

                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(14), 0.1),
                    new RoadPoint(map.GetEdge(15), 0.9),
                };

                var routes = router.Route(sources, targets, e => e.Weight, null, double.NaN);

                var paths = new Dictionary<long, long[]>() {
                    { 14L, new long[] { 0L, 4L, 8L, 14L } },
                    { 15L, new long[] { 0L, 4L, 10L, 16L, 15L } },
                };

                Assert.Equal(paths.Count(), routes.Count());

                foreach (var pair in routes)
                {
                    var route = pair.Value;
                    Assert.NotNull(paths[pair.Key.Edge.Id]);
                    var path = paths[pair.Key.Edge.Id];

                    Assert.Equal(path[0], pair.Value.Item1.Edge.Id);
                    Assert.Equal(path.Count(), route.Item2.Count());

                    int i = 0;
                    foreach (var road in route.Item2)
                    {
                        Assert.Equal((long)path[i++], road.Id);
                    }
                }
            }
            {
                // (0.7, 100) + 50 + 100 + (0.1, 200) = 240
                var sources = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(0), 0.3),
                };

                var targets = new RoadPoint[] {
                    new RoadPoint(map.GetEdge(14), 0.1),
                };
                var route = router.Route(sources, targets, e => e.Weight, e => e.Weight, 200.0);
                Assert.NotNull(route);
                Assert.Empty(route);

            }
            {
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

    }

}
