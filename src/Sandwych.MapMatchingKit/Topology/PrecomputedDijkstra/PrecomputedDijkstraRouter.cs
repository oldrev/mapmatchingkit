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
        private readonly PrecomputedDijkstraTable<long, TEdge> _precomputedTable;
        private readonly Func<TEdge, double> _cost;
        private readonly Func<TEdge, double> _boundingCost;
        private readonly double _maxRadius;

        public PrecomputedDijkstraRouter(IGraph<TEdge> graph, Func<TEdge, double> cost, Func<TEdge, double> boundingCost, double max)
        {
            _graph = graph;
            _cost = cost;
            _boundingCost = boundingCost;
            _maxRadius = max;
            _precomputedTable = this.CreateTable();
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
            throw new NotSupportedException();
        }

        public IEnumerable<TEdge> Route(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = double.NaN)
        {
            try
            {
                var edges = RouteInternal(source, target, cost, bound, max);
                if (bound != null && !double.IsNaN(max))
                {
                    var boundingDistance = edges.Sum(bound);
                    boundingDistance -= source.Edge.ComputeCost(source.Fraction, bound);
                    boundingDistance -= target.Edge.ComputeCost(1D - target.Fraction, bound);
                    if (boundingDistance > max)
                    {
                        return EmptyPath;
                    }
                }
                return edges;
            }
            catch (BadGraphPathException bgpe)
            {
                this.Logger.LogError(bgpe.Message);
                return EmptyPath;
            }
        }

        private IEnumerable<TEdge> RouteInternal(
            P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound = null, double max = Double.NaN)
        {
            IEnumerable<TEdge> edges = EmptyPath;
            //On same road && forward
            if (source.Edge.Equals(target.Edge) && source.Fraction <= target.Fraction)
            {
                edges = GetPathOnSameRoad(source.Edge);
            }
            else if (source.Edge.Target.Equals(target.Edge.Source)) //is neighborhood
            {
                edges = GetPathOnNeighborhoodRoad(source.Edge, target.Edge);
            }
            else
            {
                var t = _precomputedTable.GetPathByVertex(source.Edge.Target, target.Edge.Source);
                return CombineHeadTailEdges(source.Edge, target.Edge, t.Path);
            }

            return edges;
        }

        private PrecomputedDijkstraTable<long, TEdge> CreateTable()
        {
            var generator = new PrecomputedDijkstraTableGenerator<long, TEdge>();
            var rows = generator.ComputeRows(_graph, _cost, _boundingCost, _maxRadius);
            return new PrecomputedDijkstraTable<long, TEdge>(rows);
        }

        private static IEnumerable<TEdge> GetPathOnSameRoad(TEdge sourceEdge)
        {
            yield return sourceEdge;
            yield break;
        }

        private static IEnumerable<TEdge> GetPathOnNeighborhoodRoad(TEdge sourceEdge, TEdge targetEdge)
        {
            yield return sourceEdge;
            yield return targetEdge;
            yield break;
        }

        private static IEnumerable<TEdge> CombineHeadTailEdges(TEdge sourceEdge, TEdge targetEdge, IEnumerable<TEdge> bodyPath)
        {
            yield return sourceEdge;
            foreach (var e in bodyPath)
            {
                yield return e;
            }
            yield return targetEdge;
        }

    }

}



