using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.ShortestPath;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Utility;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraRouter<TEdge, P> : IGraphRouter<TEdge, P>
          where TEdge : class, IGraphEdge<TEdge>
          where P : IEdgePoint<TEdge>, IEquatable<P>
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly IGraph<TEdge> _graph;
        private readonly Lazy<PrecomputedDijkstraTable<long, TEdge>> _ubodt;
        private readonly Func<TEdge, double> _edgeCost;
        private readonly Func<TEdge, double> _bound;
        private readonly double _maxRadius;

        public PrecomputedDijkstraRouter(IGraph<TEdge> graph, Func<TEdge, double> cost, Func<TEdge, double> bound, double max)
        {
            _graph = graph;
            _edgeCost = cost;
            _bound = bound;
            _maxRadius = max;
            _ubodt = new Lazy<PrecomputedDijkstraTable<long, TEdge>>(() => CreateTable(), true);
        }

        public IDictionary<P, IEnumerable<TEdge>> Route(P source, IEnumerable<P> targets, Func<TEdge, double> cost,
            Func<TEdge, double> bound = null, double max = double.NaN)
        {
            throw new NotImplementedException();
        }

        public IDictionary<P, (P, IEnumerable<TEdge>)> Route(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost = null,
            Func<TEdge, double> bound = null, double max = double.NaN)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEdge> Route(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = double.NaN)
        {
            throw new NotImplementedException();
        }

        private PrecomputedDijkstraTable<long, TEdge> CreateTable()
        {
            var generator = new PrecomputedDijkstraTableGenerator<long, TEdge>();
            var rows = generator.ComputeRows(_graph, _edgeCost, _maxRadius);
            return new PrecomputedDijkstraTable<long, TEdge>(rows);
        }
    }

}



