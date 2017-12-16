using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public class DijkstraRouter<TEdge, TPoint> : IGraphRouter<TEdge, TPoint>
        where TEdge : IGraphEdge
        where TPoint : IEdgePoint<TEdge>
    {
        private readonly IGraph<TEdge> _graph;

        public DijkstraRouter(in IGraph<TEdge> graph)
        {
            _graph = graph;
        }

        public bool TryRoute(in TPoint startPoint, in TPoint endPoint, Func<TEdge, double> computeWeight, out IEnumerable<TEdge> path)
        {
            var tryFunc = _graph.InternalGraph.ShortestPathsDijkstra(edge => computeWeight(edge), startPoint.Edge.Target);
            return tryFunc(endPoint.Edge.Source, out path);
        }
    }
}
