using System;
using System.Linq;
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
            var pquery = graph.Vertices.AsParallel();
            var allRows = pquery.SelectMany(rootVertex => this.ComputeRowsSingleSource(graph, rootVertex, cost, bound, maxRadius));
            return allRows.ToList();
        }

        private IEnumerable<PrecomputedDijkstraTableRow<TVertex, TEdge>> ComputeRowsSingleSource(
            IVertexAndEdgeListGraph<TVertex, TEdge> graph,
            TVertex sourceVertex,
            Func<TEdge, double> cost, Func<TEdge, double> bound, double maxRadius)
        {
            var dijkstra = new BoundedDijkstraShortestPathAlgorithm<TVertex, TEdge>(graph, cost, bound, maxRadius);

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
