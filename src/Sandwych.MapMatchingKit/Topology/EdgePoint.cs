using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IEdgePoint<TEdge> where TEdge : IGraphEdge
    {
        TEdge Edge { get; }
        double Fraction { get; }
    }

}
