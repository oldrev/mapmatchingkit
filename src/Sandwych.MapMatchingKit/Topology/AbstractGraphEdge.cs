using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public abstract class AbstractGraphEdge<TEdge> : IGraphEdge<TEdge>
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

        public IEnumerable<TEdge> Successors
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
    }
}
