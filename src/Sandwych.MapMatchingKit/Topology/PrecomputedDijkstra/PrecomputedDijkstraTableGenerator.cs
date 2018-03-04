using System;
using System.Collections.Generic;
using QuickGraph;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraTableGenerator<TVertex, TEdge>
         where TEdge : class, IEdge<TVertex>
         where TVertex : IEquatable<TVertex>

    {

        public IEnumerable<PrecomputedDijkstraTableRow<TVertex, TEdge>> ComputeRows(
            IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            Func<TEdge, double> cost, Func<TEdge, double> bound, double maxRadius)
        {
            var dijkstra = new BoundedDijkstraShortestPathAlgorithm<TVertex, TEdge>(graph, cost, bound, maxRadius);

            foreach (var rootVertex in graph.Vertices)
            {
                var rows = this.ComputeRowsSingleSource(graph, dijkstra, rootVertex);
                foreach (var row in rows)
                {
                    yield return row;
                }
            }
        }

        private IEnumerable<PrecomputedDijkstraTableRow<TVertex, TEdge>> ComputeRowsSingleSource(
            IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            BoundedDijkstraShortestPathAlgorithm<TVertex, TEdge> dijkstra,
            TVertex sourceVertex)
        {

            try
            {
                dijkstra.Compute(sourceVertex);
            }
            catch (OutOfRadiusException)
            {
            }

            var pred = dijkstra.Predecessors;
            foreach (var u in dijkstra.VisitedVertices)
            {
                if (u.Equals(sourceVertex))
                {
                    continue;
                }

                var v = u;
                while (!pred[v].Source.Equals(sourceVertex))
                {
                    v = pred[v].Source;
                }
                graph.TryGetEdge(sourceVertex, v, out var sourceEdge);
                var targetEdge = pred[u];
                var row = new PrecomputedDijkstraTableRow<TVertex, TEdge>(sourceEdge, targetEdge, dijkstra.GetDistance(u));
                yield return row;
            }
        }
    }
}
