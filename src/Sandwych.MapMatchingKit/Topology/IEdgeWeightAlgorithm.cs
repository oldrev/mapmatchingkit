using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IEdgeWeightAlgorithm<TEdge>
        where TEdge : IGraphEdge
    {
        double ComputeWeight(in TEdge edge);
        double ComputeWeight(in TEdge edge, double fraction);
        double ComputeWeight<TPoint>(in TPoint edgePoint) where TPoint : IEdgePoint<TEdge>;
    }
}
