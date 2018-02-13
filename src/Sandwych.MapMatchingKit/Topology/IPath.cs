using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IPath<TEdge, TPoint>
        where TEdge : IGraphEdge<TEdge>
        where TPoint : struct, IEdgePoint<TEdge>
    {
        ref readonly TPoint StartPoint { get; }
        ref readonly TPoint EndPoint { get; }
        IEnumerable<TEdge> Edges { get; }
        float Length { get; }
        double Cost(Func<TEdge, double> costFunc);
    }
}
