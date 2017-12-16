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
        private readonly IEdgeWeightAlgorithm<TEdge> _edgeWeightAlgorithm;

        public DijkstraRouter(in IGraph<TEdge> graph, in IEdgeWeightAlgorithm<TEdge> edgeWeightAlgorithm)
        {
            _graph = graph;
            _edgeWeightAlgorithm = edgeWeightAlgorithm;
        }

        public bool TryRoute(in TPoint startPoint, in TPoint endPoint, out IEnumerable<TEdge> path)
        {
            var tryFunc = _graph.InternalGraph.ShortestPathsDijkstra(edge => _edgeWeightAlgorithm.ComputeWeight(edge), startPoint.Edge.Target);
            return tryFunc(endPoint.Edge.Source, out path);
        }
    }
}
