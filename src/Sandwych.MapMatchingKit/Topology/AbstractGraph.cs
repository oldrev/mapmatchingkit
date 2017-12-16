using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QuickGraph;

namespace Sandwych.MapMatchingKit.Topology
{
    public abstract class AbstractGraph<TEdge> : IGraph<TEdge>
        where TEdge : IGraphEdge
    {
        private readonly AdjacencyGraph<int, TEdge> _adjacencyGraph = new AdjacencyGraph<int, TEdge>();

        public AbstractGraph(IEnumerable<TEdge> edges)
        {
            _adjacencyGraph.AddVerticesAndEdgeRange(edges);
        }

        public IVertexAndEdgeListGraph<int, TEdge> InternalGraph => _adjacencyGraph;
    }
}
