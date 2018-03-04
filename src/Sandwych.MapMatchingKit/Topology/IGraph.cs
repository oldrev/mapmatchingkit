using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraph<TEdge> : QuickGraph.IVertexAndEdgeListGraph<long, TEdge>
        where TEdge : IGraphEdge<TEdge>
    {
        TEdge GetEdge(long id);
        IReadOnlyDictionary<long, TEdge> EdgeMap { get; }
    }
}
