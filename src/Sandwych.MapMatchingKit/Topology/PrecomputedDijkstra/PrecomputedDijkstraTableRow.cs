using QuickGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraTableRow<TVertex, TEdge>
        where TVertex : IEquatable<TVertex>
        where TEdge : class, IEdge<TVertex>
    {
        public TEdge SourceEdge { get; }
        public TEdge TargetEdge { get; }
        public double Distance { get; }

        public TVertex SourceVertex => this.SourceEdge.Source;
        public TVertex TargetVertex => this.TargetEdge.Target;
        public TVertex NextVertex => this.SourceEdge.Target;

        public PrecomputedDijkstraTableRow(TEdge s, TEdge t, double distance)
        {
            this.SourceEdge = s ?? throw new ArgumentNullException(nameof(s));
            this.TargetEdge = t ?? throw new ArgumentNullException(nameof(t));
            this.Distance = distance;
        }

        public KeyValuePair<(TVertex, TVertex), PrecomputedDijkstraTableRow<TVertex, TEdge>> ToKeyValuePair() =>
            new KeyValuePair<(TVertex, TVertex), PrecomputedDijkstraTableRow<TVertex, TEdge>>((this.SourceVertex, this.TargetVertex), this);
    }


}
