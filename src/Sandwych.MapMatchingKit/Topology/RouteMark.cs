using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    /// <summary>
    /// Route mark representation.
    /// </summary>
    internal readonly struct RouteMark<TEdge> : IComparable<RouteMark<TEdge>>, IEquatable<RouteMark<TEdge>>
        where TEdge : class, IGraphEdge<TEdge>
    {
        public TEdge MarkedEdge { get; }
        public TEdge PredecessorEdge { get; }
        public double Cost { get; }
        public double BoundingCost { get; }

        private readonly static RouteMark<TEdge> s_empty = new RouteMark<TEdge>(null, null, double.NaN, double.NaN);
        public static ref readonly RouteMark<TEdge> Empty => ref s_empty;
        public bool IsEmpty => double.IsNaN(this.Cost);

        /// <summary>
        /// Constructor of an entry.
        /// </summary>
        /// <param name="markedEdge">{@link AbstractEdge} defining the route mark.</param>
        /// <param name="predecessorEdge">Predecessor {@link AbstractEdge}.</param>
        /// <param name="cost">Cost value to this route mark.</param>
        /// <param name="boundingCost">Bounding cost value to this route mark.</param>
        public RouteMark(TEdge markedEdge, TEdge predecessorEdge, Double cost, Double boundingCost)
        {
            this.MarkedEdge = markedEdge;
            this.PredecessorEdge = predecessorEdge;
            this.Cost = cost;
            this.BoundingCost = boundingCost;
        }

        public int CompareTo(RouteMark<TEdge> other)
        {
            if (this.IsEmpty || other.IsEmpty)
            {
                throw new InvalidOperationException();
            }

            if (this.Cost < other.Cost)
            {
                return -1;
            }
            else if (this.Cost > other.Cost)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override int GetHashCode() =>
            (this.MarkedEdge, this.PredecessorEdge, this.Cost, this.BoundingCost).GetHashCode();

        public bool Equals(RouteMark<TEdge> other)
        {
            if (this.IsEmpty || other.IsEmpty)
            {
                throw new InvalidOperationException();
            }
            return this.CompareTo(other) == 0;
        }
    }
}
