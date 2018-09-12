using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IEdgePoint<TEdge>
        where TEdge : IGraphEdge<TEdge>, IEquatable<TEdge>
    {
        TEdge Edge { get; }
        double Fraction { get; }
    }

}
