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
        private readonly static IEnumerable<TEdge> EmptyPath = new TEdge[] { };

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

        public bool HasPathByVertex(TVertex sourceVertex, TVertex targetVertex)
        {
            if (IsSameVertex(sourceVertex, targetVertex))
            {
                return false;
            }
            else
            {
                return this.ContainsKey((sourceVertex, targetVertex));
            }
        }

        public bool HasPathByEdge(TEdge sourceEdge, TEdge targetEdge)
        {
            if (IsSameEdge(sourceEdge, targetEdge))
            {
                throw new InvalidOperationException();
            }
            else if (IsNeighbor(sourceEdge, targetEdge))
            {
                return true;
            }
            else
            {
                return this.HasPathByVertex(sourceEdge.Target, targetEdge.Source);
            }
        }

        public (IEnumerable<TEdge> Path, double Distance) GetPathByVertex(TVertex sourceVertex, TVertex targetVertex, double maxDistance = double.PositiveInfinity)
        {
            if (IsSameVertex(sourceVertex, targetVertex))
            {
                throw new ArgumentException();
            }

            if (this.TryGetValue((sourceVertex, targetVertex), out var firstRow) && firstRow.Distance <= maxDistance)
            {
                return (this.GetPathFrom(firstRow), firstRow.Distance);
            }
            else
            {
                return (EmptyPath, double.NaN);
            }
        }

        private IEnumerable<TEdge> GetPathFrom(PrecomputedDijkstraTableRow<TVertex, TEdge> startRow)
        {
            var row = startRow;
            //now we got the first edge, then we started from the next row
            yield return row.SourceEdge;
            var currentStart = row.NextVertex;
            var sourceVertex = startRow.SourceVertex;
            var targetVertex = startRow.TargetVertex;

            while (!currentStart.Equals(targetVertex))
            {
                if (this.TryGetValue((currentStart, targetVertex), out row))
                {
                    yield return row.SourceEdge;
                    currentStart = row.NextVertex;
                }
                else
                {
                    var msg = string.Format(
                        "Bad precomputed Dijkstra path: [sourceVertex={0}, targetVertex={1}, currentVertex={2}]",
                        sourceVertex, targetVertex, currentStart);
                    throw new BadGraphPathException(msg);
                }
            }
        }

        private static bool IsSameVertex(TVertex sourceVertex, TVertex targetVertex) =>
            sourceVertex.Equals(targetVertex);

        private static bool IsNeighbor(TEdge sourceEdge, TEdge targetEdge) =>
            sourceEdge.Target.Equals(targetEdge.Source);

        private static bool IsSameEdge(TEdge sourceEdge, TEdge targetEdge) =>
            sourceEdge.Source.Equals(targetEdge.Source) && sourceEdge.Target.Equals(targetEdge.Target);

    }
}
