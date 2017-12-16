using QuickGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraph<TEdge>
        where TEdge : IGraphEdge
    {
        IVertexAndEdgeListGraph<int, TEdge> InternalGraph { get; }
    }
}
