using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{

    public sealed class PrecomputedDijkstraTable<TVertex, TEdge> : Dictionary<(TVertex, TVertex), PrecomputedDijkstraTableRow<TVertex, TEdge>>
        where TEdge : class, IEdge<TVertex>
        where TVertex : IEquatable<TVertex>
    {
        public PrecomputedDijkstraTable() : base()
        {

        }

        public PrecomputedDijkstraTable(IEnumerable<PrecomputedDijkstraTableRow<TVertex, TEdge>> rows) :
            this(rows.Count())
        {
            foreach (var row in rows)
            {
                var pair = row.ToKeyValuePair();
                this.Add(pair.Key, pair.Value);
            }
        }

        public PrecomputedDijkstraTable(int capacity) : base(capacity)
        {
        }

        public IEnumerable<TEdge> GetPathByEdge(TEdge sourceEdge, TEdge targetEdge)
        {
            if (sourceEdge == targetEdge)
            {
                yield break;
            }
            else if (sourceEdge.Target.Equals(targetEdge.Source))
            {
                yield return sourceEdge;
                yield return targetEdge;
                yield break;
            }
            else
            {
                var sourceVertex = sourceEdge.Target;
                var targetVertex = targetEdge.Source;
                var path = this.GetPathByVertex(sourceVertex, targetVertex);
                foreach (var edge in path)
                {
                    yield return edge;
                }
            }
        }

        public IEnumerable<TEdge> GetPathByVertex(TVertex sourceVertex, TVertex targetVertex)
        {
            if (this.TryGetValue((sourceVertex, targetVertex), out var row))
            {
                //now we got the first edge, then we started from the next row
                yield return row.SourceEdge;
                var currentStart = row.NextVertex;

                while (!currentStart.Equals(targetVertex))
                {
                    row = this[(currentStart, targetVertex)];
                    yield return row.SourceEdge;
                    currentStart = row.NextVertex;
                }
            }
            else
            {
                yield break;
            }
        }

    }
}
