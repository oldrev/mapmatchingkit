using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IEdgeWeightAlgorithm<TEdge> where TEdge : IGraphEdge
    {
        double ComputeWeight(TEdge edge);
    }
}
