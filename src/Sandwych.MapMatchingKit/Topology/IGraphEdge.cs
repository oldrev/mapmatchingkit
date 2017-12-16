using QuickGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraphEdge : IEdge<int>
    {
        int Id { get; }
    }
}
