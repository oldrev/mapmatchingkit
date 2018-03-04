using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Utility;

namespace Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraRouter<TEdge, P> : IGraphRouter<TEdge, P>
          where TEdge : class, IGraphEdge<TEdge>, IEquatable<TEdge>
          where P : IEdgePoint<TEdge>, IEquatable<P>
    {
        private static readonly TEdge[] EmptyPath = new TEdge[] { };
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly IGraph<TEdge> _graph;
        private readonly PrecomputedDijkstraTable<long, TEdge> _ubodt;
        private readonly Func<TEdge, double> _cost;
        private readonly Func<TEdge, double> _boundingCost;
        private readonly double _maxRadius;

        public PrecomputedDijkstraRouter(IGraph<TEdge> graph, Func<TEdge, double> cost, Func<TEdge, double> boundingCost, double max)
        {
            _graph = graph;
            _cost = cost;
            _boundingCost = boundingCost;
            _maxRadius = max;
            _ubodt = this.CreateTable();
        }

        public IReadOnlyDictionary<P, IEnumerable<TEdge>> Route(P source, IEnumerable<P> targets, Func<TEdge, double> cost,
            Func<TEdge, double> bound = null, double max = double.NaN)
        {
            var dict = new Dictionary<P, IEnumerable<TEdge>>(targets.Count());
            foreach (var target in targets)
            {
                var path = this.Route(source, target, cost, bound, max);
                if (path != null && path.Count() > 0)
                {
                    dict.Add(target, path);
                }
            }
            return dict;
        }

        public IReadOnlyDictionary<P, (P, IEnumerable<TEdge>)> Route(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost = null,
            Func<TEdge, double> bound = null, double max = double.NaN)
        {
            var result = new Dictionary<P, (P, IEnumerable<TEdge>)>();
            foreach (var source in sources)
            {
                var ssmtResult = this.Route(source, targets, cost, bound, max);
                foreach (var r in ssmtResult)
                {
                    result.Add(r.Key, (source, r.Value));
                }
            }
            return result;
        }

        public IEnumerable<TEdge> Route(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = double.NaN)
        {
            var edges = RouteInternal(source, target, cost, bound, max);
            if (bound != null && !double.IsNaN(max))
            {
                var boundingDistance = edges.Sum(bound);
                boundingDistance -= source.Edge.Cost(source.Fraction, bound);
                boundingDistance -= target.Edge.Cost(1D - target.Fraction, bound);
                if (boundingDistance > max)
                {
                    return EmptyPath;
                }
            }
            return edges;
        }

        private IEnumerable<TEdge> RouteInternal(
            P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = Double.NaN)
        {
            //On Same Road and forward
            if (source.Edge.Equals(target.Edge) && source.Fraction <= target.Fraction)
            {
                yield return source.Edge;
                yield break;
            }
            else if (source.Edge.Target.Equals(target.Edge.Source)) //is neighborhood
            {
                yield return source.Edge;
                yield return target.Edge;
                yield break;
            }

            var edges = _ubodt.GetPathByEdge(source.Edge, target.Edge);
            if (this.CheckPathBoundingCost(edges, bound, max))
            {
                foreach (var e in edges)
                {
                    yield return e;
                }
                yield break;
            }
        }

        private bool CheckPathBoundingCost(IEnumerable<TEdge> path, Func<TEdge, double> bound = null, double max = double.NaN)
        {
            if (bound != null && !double.IsNaN(max))
            {
                var bounding = path.Sum(this._boundingCost);
                return (bounding <= max);
            }
            else
            {
                return true;
            }
        }

        private PrecomputedDijkstraTable<long, TEdge> CreateTable()
        {
            var generator = new PrecomputedDijkstraTableGenerator<long, TEdge>();
            var rows = generator.ComputeRows(_graph, _cost, _boundingCost, _maxRadius);
            return new PrecomputedDijkstraTable<long, TEdge>(rows);
        }

    }

}



