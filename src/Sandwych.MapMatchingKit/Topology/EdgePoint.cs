using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IEdgePoint<TEdge>
        where TEdge : IGraphEdge<TEdge>
    {
        TEdge Road { get; }
        double Fraction { get; }
    }

}
