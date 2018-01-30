using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sandwych.MapMatchingKit.Topology
{
    public interface IGraphRouter<TEdge, TPoint>
        where TEdge : IGraphEdge<TEdge>
        where TPoint : IEdgePoint<TEdge>
    {
        bool TryRoute(in TPoint startPoint, in TPoint endPoint, Func<TEdge, double> computeWeight, out IEnumerable<TEdge> path);
        bool TryRoute(in TPoint source, ISet<TPoint> targets, Func<TEdge, double> cost, Func<TEdge, double> bound, double max, out IDictionary<TPoint, IEnumerable<TEdge>> paths);
    }

    public static class IGraphRouterExtensions
    {
        public static IReadOnlyDictionary<TPoint, IEnumerable<TEdge>> Route<TEdge, TPoint>
            (this IGraphRouter<TEdge, TPoint> self, in TPoint startPoint, in IEnumerable<TPoint> endPoints, Func<TEdge, double> computeWeight)
        where TEdge : IGraphEdge<TEdge>
        where TPoint : IEdgePoint<TEdge>
        {
            var pathCollection = new Dictionary<TPoint, IEnumerable<TEdge>>(endPoints.Count());
            foreach (var endPoint in endPoints)
            {
                var result = self.TryRoute(in startPoint, in endPoint, computeWeight, out var path);
                if (result)
                {
                    pathCollection.Add(endPoint, path);
                }
            }
            return pathCollection;
        }

    }

}
