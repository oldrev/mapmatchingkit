using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IPath<TEdge, TPoint>
        where TEdge : IGraphEdge
        where TPoint : IEdgePoint<TEdge>
    {
        TPoint StartPoint { get; }
        TPoint EndPoint { get; }
        IEnumerable<TEdge> Edges { get; }
        double Distance { get; }
    }
}
