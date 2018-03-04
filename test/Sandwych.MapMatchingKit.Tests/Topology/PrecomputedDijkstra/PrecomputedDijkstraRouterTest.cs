using System;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Topology;
using Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra;

namespace Sandwych.MapMatchingKit.Tests.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraRouterTest : AbstractRouterTest
    {
        protected override IGraphRouter<Road, RoadPoint> CreateRouter(
            Graph graph, Func<Road, double> cost, Func<Road, double> bound, double max) =>
            new PrecomputedDijkstraRouter<Road, RoadPoint>(graph, cost, bound, max);
    }
}
