using System;
using System.Collections.Generic;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{

    public class BoundedDijkstraShortestPathAlgorithm<TVertex, TEdge>
        where TVertex : IEquatable<TVertex>
        where TEdge : IEdge<TVertex>
    {
        private readonly HashSet<TVertex> _visitedVertices = new HashSet<TVertex>();
        private readonly Dictionary<TVertex, TEdge> _vertexPredecessors = new Dictionary<TVertex, TEdge>();

        public DijkstraShortestPathAlgorithm<TVertex, TEdge> Algorithm { get; }
        public double MaxRadius { get; }
        public IEnumerable<TVertex> VisitedVertices => _visitedVertices;
        public IReadOnlyDictionary<TVertex, TEdge> Predecessors => _vertexPredecessors;

        public BoundedDijkstraShortestPathAlgorithm(IVertexAndEdgeListGraph<TVertex, TEdge> graph, Func<TEdge, double> cost, double maxRadius)
        {
            this.Algorithm = new DijkstraShortestPathAlgorithm<TVertex, TEdge>(graph, cost);
            this.Algorithm.ExamineVertex += this.ExamineVertex;
            this.Algorithm.TreeEdge += this.OnTreeEdge;
            this.MaxRadius = maxRadius;
        }

        public void Compute(TVertex rootVertex)
        {
            _vertexPredecessors.Clear();
            _visitedVertices.Clear();
            this.Algorithm.Compute(rootVertex);
        }

        public double GetDistance(TVertex vertex) =>
            this.Algorithm.Distances[vertex];

        public bool TryGetDistance(TVertex vertex, out double distance) =>
            this.Algorithm.TryGetDistance(vertex, out distance);

        private void ExamineVertex(TVertex vertex)
        {
            if (this.Algorithm.Distances[vertex] > this.MaxRadius)
            {
                throw new OutOfRadiusException();
            }

            this._visitedVertices.Add(vertex);
        }

        private void OnTreeEdge(TEdge e) =>
            _vertexPredecessors[e.Target] = e;

        public bool TryGetPath(TVertex vertex, out IEnumerable<TEdge> path) =>
            this._vertexPredecessors.TryGetPath(vertex, out path);

    }
}
