using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Topology;
using Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraRouterTest : TestBase
    {
        private readonly Graph _map;

        public PrecomputedDijkstraRouterTest()
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
            _map = new Graph(roads);
        }

        [Fact]
        public void ShortestPathTest1()
        {
            var max = 200D;
            var start = new RoadPoint(_map.GetEdge(0), 0.3);
            var targets = new RoadPoint[] {
                new RoadPoint(_map.GetEdge(10), 0.5),
                new RoadPoint(_map.GetEdge(17), 0.6),
            };
            Func<Road, double> cost = r => r.Weight;

            var dijkstraRouter = new DijkstraRouter<Road, RoadPoint>();
            var precomputedRouter = new PrecomputedDijkstraRouter<Road, RoadPoint>(_map, cost, cost, max);

            var expectedPath = dijkstraRouter.Route(start, targets, cost, cost, max);
            var actualPath = precomputedRouter.Route(start, targets, cost, cost, max);

            Assert.Single(actualPath);
            Assert.Equal(expectedPath, actualPath);
        }

    }
}
