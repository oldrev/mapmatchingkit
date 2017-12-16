using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public abstract class AbstractEdgeWeightAlgorithm<TEdge> : IEdgeWeightAlgorithm<TEdge>
        where TEdge : IGraphEdge
    {
        public abstract double ComputeWeight(in TEdge edge);

        public double ComputeWeight(in TEdge edge, double fraction) =>
            this.ComputeWeight(edge) * fraction;

        public double ComputeWeight<TPoint>(in TPoint edgePoint)
            where TPoint : IEdgePoint<TEdge> =>
            this.ComputeWeight(edgePoint.Edge, edgePoint.Fraction);
    }
}
