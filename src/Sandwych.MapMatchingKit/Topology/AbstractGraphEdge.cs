using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{

    /// <summary>
    /// Abstract edge in a directed <see cref="Topology.AbstractGraph{TEdge}"/>.
    /// </summary>
    /// <typeparam name="TEdge">Implementation of <see cref="Topology.AbstractGraph{TEdge}" /> in a directed graph. 
    /// (Use the curiously recurring template pattern (CRTP) for type-safe use of customized edge type.)
    /// </typeparam>
    public abstract class AbstractGraphEdge<TEdge> : IGraphEdge<TEdge>, IEquatable<TEdge>
        where TEdge : class, IGraphEdge<TEdge>
    {
        public long Id { get; }

        public long Source { get; }

        public long Target { get; }

        public TEdge Neighbor { get; internal set; }

        public TEdge Successor { get; internal set; }


        public AbstractGraphEdge(long id, long source, long target)
        {
            this.Id = id;
            this.Source = source;
            this.Target = target;
        }

        public virtual IEnumerable<TEdge> Successors
        {
            get
            {
                var s = this.Successor;
                var i = s;
                while (i != null)
                {
                    if (i == null)
                    {
                        yield return null;
                    }
                    else
                    {
                        var next = i;
                        i = i.Neighbor == s ? null : i.Neighbor;
                        yield return next;
                    }
                }
            }
        }

        public override int GetHashCode() => this.Id.GetHashCode();

        public virtual bool Equals(TEdge other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Id == other.Id;
        }
    }
}
