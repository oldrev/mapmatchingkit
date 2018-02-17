using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Utility;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Sandwych.MapMatchingKit.Topology
{

    /// <summary>
    /// Dijkstra's algorithm implementation of a <see cref="Sandwych.MapMatchingKit.Topology.IGraphRouter{TEdge, P}" />. 
    /// The routing functions use the Dijkstra algorithm for finding shortest paths according to a customizable cost function.
    /// </summary>
    /// <typeparam name="TEdge">Implementation of <see cref="Sandwych.MapMatchingKit.Topology.IGraphEdge{T}"> 
    /// in a directed <see cref="Sandwych.MapMatchingKit.Topology.IGraph{TEdge}" />.</typeparam>
    /// <typeparam name="P"><see cref="Sandwych.MapMatchingKit.Topology.IEdgePoint{TEdge}"/> type of positions in the network.</typeparam>
    public class DijkstraRouter<TEdge, P> : IGraphRouter<TEdge, P>
        where TEdge : class, IGraphEdge<TEdge>
        where P : IEdgePoint<TEdge>
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        /// <summary>
        /// Route mark representation.
        /// </summary>
        private sealed class Mark : IComparable<Mark>
        {
            public TEdge MarkedEdge { get; }
            public TEdge PredecessorEdge { get; }
            public double Cost { get; }
            public double BoundingCost { get; }

            /// <summary>
            /// Constructor of an entry.
            /// </summary>
            /// <param name="markedEdge">{@link AbstractEdge} defining the route mark.</param>
            /// <param name="predecessorEdge">Predecessor {@link AbstractEdge}.</param>
            /// <param name="cost">Cost value to this route mark.</param>
            /// <param name="boundingCost">Bounding cost value to this route mark.</param>
            public Mark(TEdge markedEdge, TEdge predecessorEdge, Double cost, Double boundingCost)
            {
                this.MarkedEdge = markedEdge;
                this.PredecessorEdge = predecessorEdge;
                this.Cost = cost;
                this.BoundingCost = boundingCost;
            }

            public int CompareTo(Mark other)
            {
                if (this.Cost < other.Cost)
                {
                    return -1;
                }
                else if (this.Cost > other.Cost)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            public override int GetHashCode() =>
                (this.MarkedEdge, this.PredecessorEdge, this.Cost, this.BoundingCost).GetHashCode();
        }

        public IEnumerable<TEdge> Route(P source, P target, Func<TEdge, double> cost)
        {
            return Ssst(source, target, cost, null, double.NaN);
        }

        public IEnumerable<TEdge> Route(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound, double max)
        {
            return Ssst(source, target, cost, bound, max);
        }

        public IDictionary<P, IEnumerable<TEdge>> Route(P source, IEnumerable<P> targets, Func<TEdge, double> cost)
        {
            return Ssmt(source, targets, cost, null, double.NaN);
        }

        public IDictionary<P, IEnumerable<TEdge>> Route(P source, IEnumerable<P> targets, Func<TEdge, double> cost,
            Func<TEdge, double> bound = null, double max = double.NaN)
        {
            return Ssmt(source, targets, cost, bound, max);
        }

        public IDictionary<P, (P, IEnumerable<TEdge>)> Route(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost,
            Func<TEdge, double> bound, double max)
        {
            return Msmt(sources, targets, cost, bound, max);
        }

        private IEnumerable<TEdge> Ssst(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound, double max)
        {
            var targets = new P[] { target };
            return Ssmt(source, targets, cost, bound, max)[target];
        }

        private IDictionary<P, IEnumerable<TEdge>> Ssmt(P source, IEnumerable<P> targets, Func<TEdge, double> cost, Func<TEdge, double> bound, double max = double.NaN)
        {
            var sources = new P[1] { source };
            var map = Msmt(sources, targets, cost, bound, max);
            var result = new Dictionary<P, IEnumerable<TEdge>>(map.Count);
            foreach (var entry in map)
            {
                result[entry.Key] = entry.Value.Item2;
            }
            return result;
        }

        private IDictionary<P, (P, IEnumerable<TEdge>)> Msmt(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost,
                Func<TEdge, double> bound, double max)
        {
            /*
             * Initialize map of edges to target points.
             */
            var targetEdges = new Dictionary<TEdge, HashSet<P>>();
            foreach (var target in targets)
            {
                if (this.Logger.IsEnabled(LogLevel.Debug))
                {
                    this.Logger.LogDebug("initialize target {0} with edge {1} and fraction {2}",
                        target, target.Edge.Id, target.Fraction);
                }

                if (!targetEdges.ContainsKey(target.Edge))
                {
                    targetEdges[target.Edge] = new HashSet<P>() { target };
                }
                else
                {
                    targetEdges[target.Edge].Add(target);
                }
            }

            /*
             * Setup data structures
             */
            var priorities = new PriorityQueue<Mark>();
            var entries = new Dictionary<TEdge, Mark>();
            var finishs = new Dictionary<P, Mark>();
            var reaches = new Dictionary<Mark, P>();
            var starts = new Dictionary<Mark, P>();

            /*
             * Initialize map of edges with start points
             */
            foreach (var source in sources)
            {
                // initialize sources as start edges
                var startcost = source.Edge.Cost(1.0 - source.Fraction, cost); //cost.cost(source.Edge, 1 - source.Fraction);
                var startbound = bound != null ? source.Edge.Cost(1.0 - source.Fraction, bound) : 0D;  //bound.cost(source.Edge.Cost(), 1 - source.Fraction) : 0.0;

                if (this.Logger.IsEnabled(LogLevel.Debug))
                {
                    this.Logger.LogDebug("init source {0} with start edge {1} and fraction {2} with {3} cost",
                        source, source.Edge.Id, source.Fraction, startcost);
                }

                if (targetEdges.TryGetValue(source.Edge, out var targetsMap))
                {
                    // start edge reaches target edge
                    foreach (var target in targetsMap)
                    {
                        if (target.Fraction < source.Fraction)
                        {
                            continue;
                        }
                        var reachcost = startcost - source.Edge.Cost(1.0 - target.Fraction, cost); // cost.cost(source.Edge, 1 - target.Fraction);
                        var reachbound = bound != null ? startcost - source.Edge.Cost(1.0 - target.Fraction, bound) : 0D; //, // bound.cost(source.Edge, 1 - target.Fraction) : 0.0;

                        if (this.Logger.IsEnabled(LogLevel.Debug))
                        {
                            this.Logger.LogDebug("reached target {0} with start edge {1} from {2} to {3} with {4} cost",
                                target, source.Edge.Id, source.Fraction, target.Fraction, reachcost);
                        }

                        var reach = new Mark(source.Edge, null, reachcost, reachbound);
                        reaches.Add(reach, target);
                        starts.Add(reach, source);
                        priorities.Enqueue(reach);
                    }
                }

                if (!entries.TryGetValue(source.Edge, out var start))
                {
                    if (this.Logger.IsEnabled(LogLevel.Debug))
                    {
                        this.Logger.LogDebug("add source {0} with start edge {1} and fraction {2} with {3} cost",
                            source, source.Edge.Id, source.Fraction, startcost);
                    }
                    start = new Mark(source.Edge, null, startcost, startbound);
                    entries[source.Edge] = start;
                    starts[start] = source;
                    priorities.Enqueue(start);
                }
                else
                {
                    if (startcost < start.Cost)
                    {
                        if (this.Logger.IsEnabled(LogLevel.Debug))
                        {
                            this.Logger.LogDebug("update source {0} with start edge {1} and fraction {2} with {3} cost",
                                source, source.Edge.Id, source.Fraction, startcost);
                        }
                        start = new Mark(source.Edge, null, startcost, startbound);
                        entries[source.Edge] = start;
                        starts[start] = source;
                        priorities.Remove(start);
                        priorities.Enqueue(start);
                    }
                }
            }

            /*
             * Dijkstra algorithm.
             */
            while (priorities.Count > 0)
            {
                var current = priorities.Dequeue();

                if (targetEdges.Count == 0)
                {
                    if (this.Logger.IsEnabled(LogLevel.Debug))
                    {
                        this.Logger.LogDebug("finshed all targets");
                    }
                    break;
                }

                if (!double.IsNaN(max) && current.BoundingCost > max)
                {
                    if (this.Logger.IsEnabled(LogLevel.Debug))
                    {
                        this.Logger.LogDebug("reached maximum bound");
                    }
                    continue;
                }

                /*
                 * Finish target if reached.
                 */
                {
                    if (reaches.TryGetValue(current, out var target))
                    {
                        if (finishs.ContainsKey(target))
                        {
                            continue;
                        }
                        else
                        {
                            if (this.Logger.IsEnabled(LogLevel.Debug))
                            {
                                this.Logger.LogDebug("finished target {0} with edge {1} and fraction {2} with {3} cost",
                                    target, current.MarkedEdge, target.Fraction, current.Cost);
                            }

                            finishs[target] = current;

                            var edges = targetEdges[current.MarkedEdge];
                            edges.Remove(target);

                            if (edges.Count == 0)
                            {
                                targetEdges.Remove(current.MarkedEdge);
                            }
                            continue;
                        }
                    }
                }

                if (this.Logger.IsEnabled(LogLevel.Debug))
                {
                    this.Logger.LogDebug("succeed edge {0} with {1} cost", current.MarkedEdge.Id, current.Cost);
                }

                var successors = current.MarkedEdge.Successors;

                foreach (var successor in successors)
                {
                    var succcost = current.Cost + cost(successor);
                    var succbound = bound != null ? current.BoundingCost + bound(successor) : 0.0;

                    if (targetEdges.ContainsKey(successor))
                    {
                        // reach target edge
                        foreach (var targetEdge in targetEdges[successor])
                        {
                            var reachcost = succcost - successor.Cost(1.0 - targetEdge.Fraction, cost);
                            var reachbound = bound != null ? succbound - successor.Cost(1.0 - targetEdge.Fraction, bound) : 0.0;    /// bound(successor, 1 - target.Fraction) : 0.0;
                            if (this.Logger.IsEnabled(LogLevel.Debug))
                            {
                                this.Logger.LogDebug("reached target {0} with successor edge {1} and fraction {2} with {3} cost",
                                    targetEdge, successor.Id, targetEdge.Fraction, reachcost);
                            }

                            var reach = new Mark(successor, current.MarkedEdge, reachcost, reachbound);
                            reaches.Add(reach, targetEdge);
                            priorities.Enqueue(reach);
                        }
                    }

                    if (!entries.ContainsKey(successor))
                    {
                        if (this.Logger.IsEnabled(LogLevel.Debug))
                        {
                            this.Logger.LogDebug("added successor edge {0} with {1} cost", successor.Id, succcost);
                        }
                        Mark mark = new Mark(successor, current.MarkedEdge, succcost, succbound);
                        entries.Add(successor, mark);
                        priorities.Enqueue(mark);
                    }
                }
            }

            var paths = new Dictionary<P, (P, IEnumerable<TEdge>)>();

            foreach (P targetPoint in targets)
            {
                if (finishs.ContainsKey(targetPoint))
                {
                    var path = new List<TEdge>();
                    var iterator = finishs[targetPoint];
                    Mark start = null;
                    while (iterator != null)
                    {
                        path.Add(iterator.MarkedEdge);
                        start = iterator;
                        iterator = iterator.PredecessorEdge != null ? entries.GetOrNull(iterator.PredecessorEdge) : null;
                    }
                    path.Reverse();
                    var tp = (starts[start], path);
                    paths.Add(targetPoint, tp);
                }
            }

            /*
            entries.Clear();
            finishs.Clear();
            reaches.Clear();
            priorities.Clear();
            */

            return paths;
        }
    }
}
