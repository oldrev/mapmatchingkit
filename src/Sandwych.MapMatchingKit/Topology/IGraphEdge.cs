using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraphEdge<T>
        where T : IGraphEdge<T>
    {
        long Id { get; }

        long Source { get; }

        long Target { get; }

        T Neighbor { get; }

        T Successor { get; }

        IEnumerable<T> Successors { get; }
    }
}
