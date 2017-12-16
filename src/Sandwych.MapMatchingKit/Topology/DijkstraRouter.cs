using QuickGraph.Algorithms.ShortestPath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public class DijkstraRouter<TEdge> : IGraphRouter<TEdge>
        where TEdge : IGraphEdge
    {
        private readonly DijkstraShortestPathAlgorithm<int, TEdge> _dijkstra;
        private readonly IGraph<TEdge> _graph;
        private readonly IEdgeWeightAlgorithm<TEdge> _edgeWeightAlgorithm;

        public DijkstraRouter(IGraph<TEdge> graph, IEdgeWeightAlgorithm<TEdge> edgeWeightAlgorithm)
        {
            _graph = graph;
            _edgeWeightAlgorithm = edgeWeightAlgorithm;
            _dijkstra = new DijkstraShortestPathAlgorithm<int, TEdge>(_graph.InternalGraph, edge => _edgeWeightAlgorithm.ComputeWeight(edge));
        }

        public bool TryRoute(TEdge source, TEdge target, out IEnumerable<IGraphEdge> path)
        {
            throw new NotImplementedException();
        }
    }
}
