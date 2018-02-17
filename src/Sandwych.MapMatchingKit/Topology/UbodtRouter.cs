using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Utility;

namespace Sandwych.MapMatchingKit.Topology
{
    public class UbodtRouter<TEdge, P> : IGraphRouter<TEdge, P>
          where TEdge : class, IGraphEdge<TEdge>
          where P : IEdgePoint<TEdge>
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public IDictionary<P, IEnumerable<TEdge>> Route(P source, IEnumerable<P> targets, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = double.NaN)
        {
            throw new NotImplementedException();
        }

    }

}
