using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraphRouter<TEdge> where TEdge : IGraphEdge
    {
        bool TryRoute(TEdge source, TEdge target, out IEnumerable<IGraphEdge> path);
    }

}
