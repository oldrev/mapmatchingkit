using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraph<TEdge>
        where TEdge : IGraphEdge<TEdge>
    {
        TEdge GetEdge(long id);
        IReadOnlyDictionary<long, TEdge> Edges { get; }
    }
}
