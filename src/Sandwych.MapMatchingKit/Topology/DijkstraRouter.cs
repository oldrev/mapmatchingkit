using Castle.Core.Logging;
using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;

namespace Sandwych.MapMatchingKit.Topology
{

    /*
     * Dijkstra's algorithm implementation of a {@link Router}. The routing functions use the Dijkstra
     * algorithm for finding shortest paths according to a customizable {@link Cost} function.
     *
     * @param <E> Implementation of {@link AbstractEdge} in a directed {@link Graph}.
     * @param <P> {@link Point} type of positions in the network.
     */
    public class DijkstraRouter<P, TEdge>
        where TEdge : class, IGraphEdge<TEdge>
        where P : IEdgePoint<TEdge>
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;
        /*
        *  Route mark representation.
        */
        private class Mark : IComparable<Mark>
        {
            /*
             * Constructor of an entry.
             *
             * @param one {@link AbstractEdge} defining the route mark.
             * @param two Predecessor {@link AbstractEdge}.
             * @param three Cost value to this route mark.
             * @param four Bounding cost value to this route mark.
             */
            public TEdge One { get; }
            public TEdge Two { get; }
            public double Cost { get; }
            public double Bound { get; }

            public Mark(TEdge one, TEdge two, Double cost, Double bound)
            {
                this.One = one;
                this.Two = two;
                this.Cost = cost;
                this.Bound = bound;
            }

            public int CompareTo(Mark other) =>
                (this.Cost < other.Cost) ? -1 : (this.Cost > other.Cost) ? 1 : 0;

            public override int GetHashCode() =>
                (this.One, this.Two, this.Cost, this.Bound).GetHashCode();
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
            Func<TEdge, double> bound, double max)
        {
            return Ssmt(source, targets, cost, bound, max);
        }

        public IDictionary<P, (P, IEnumerable<TEdge>)> Route(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost)
        {
            return msmt(sources, targets, cost, null, double.NaN);
        }

        public IDictionary<P, (P, IEnumerable<TEdge>)> Route(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost,
            Func<TEdge, double> bound, double max)
        {
            return msmt(sources, targets, cost, bound, max);
        }

        private IEnumerable<TEdge> Ssst(P source, P target, Func<TEdge, double> cost, Func<TEdge, double> bound, double max)
        {
            var targets = new P[] { target };
            return Ssmt(source, targets, cost, bound, max)[target];
        }

        private IDictionary<P, IEnumerable<TEdge>> Ssmt(P source, IEnumerable<P> targets, Func<TEdge, double> cost, Func<TEdge, double> bound, double max = double.NaN)
        {
            var sources = new P[] { source };
            var map = msmt(sources, targets, cost, bound, max);
            var result = new Dictionary<P, IEnumerable<TEdge>>(map.Count);
            foreach (var entry in map)
            {
                result[entry.Key] = entry.Value.Item2;
            }
            return result;
        }

        private IDictionary<P, (P, IEnumerable<TEdge>)> msmt(IEnumerable<P> sources, IEnumerable<P> targets, Func<TEdge, double> cost,
                Func<TEdge, double> bound, double max)
        {
            /*
             * Initialize map of edges to target points.
             */
            var targetEdges = new Dictionary<TEdge, HashSet<P>>();
            foreach (var target in targets)
            {
                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.DebugFormat("initialize target {0} with edge {1} and fraction {2}",
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
            var priorities = new C5.TreeBag<Mark>();
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
                var startcost = source.Edge.Cost(1 - source.Fraction, cost); //cost.cost(source.Edge, 1 - source.Fraction);
                var startbound = bound != null ? source.Edge.Cost(1 - source.Fraction, bound) : 0;  //bound.cost(source.Edge.Cost(), 1 - source.Fraction) : 0.0;

                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.DebugFormat("init source {0} with start edge {1} and fraction {2} with {3} cost",
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
                        var reachbound = bound != null ? startcost - source.Edge.Cost(1.0 - target.Fraction, bound) : 0.0; //, // bound.cost(source.Edge, 1 - target.Fraction) : 0.0;

                        if (this.Logger.IsDebugEnabled)
                        {
                            this.Logger.DebugFormat("reached target {0} with start edge {1} from {2} to {3} with {4} cost",
                                target, source.Edge.Id, source.Fraction, target.Fraction, reachcost);

                        }

                        var reach = new Mark(source.Edge, null, reachcost, reachbound);
                        reaches.Add(reach, target);
                        starts.Add(reach, source);
                        priorities.Add(reach);
                    }
                }

                if (!entries.TryGetValue(source.Edge, out var start))
                {
                    if (this.Logger.IsDebugEnabled)
                    {
                        this.Logger.DebugFormat("add source {0} with start edge {1} and fraction {2} with {3} cost",
                            source, source.Edge.Id, source.Fraction, startcost);
                    }
                    start = new Mark(source.Edge, null, startcost, startbound);
                    entries[source.Edge] = start;
                    starts[start] = source;
                    priorities.Add(start);
                }
                else
                {
                    if (startcost < start.Cost)
                    {
                        if (this.Logger.IsDebugEnabled)
                        {
                            this.Logger.DebugFormat("update source {0} with start edge {1} and fraction {2} with {3} cost",
                                source, source.Edge.Id, source.Fraction, startcost);
                        }
                        start = new Mark(source.Edge, null, startcost, startbound);
                        entries[source.Edge] = start;
                        starts[start] = source;
                        priorities.Remove(start);
                        priorities.Add(start);
                    }
                }
            }

            /*
             * Dijkstra algorithm.
             */
            while (priorities.Count > 0)
            {
                var current = priorities.DeleteMin();

                if (targetEdges.Count == 0)
                {
                    if (this.Logger.IsDebugEnabled)
                    {
                        this.Logger.Debug("finshed all targets");
                    }
                    break;
                }

                if (!double.IsNaN(max) && current.Bound > max)
                {
                    if (this.Logger.IsDebugEnabled)
                    {
                        this.Logger.Debug("reached maximum bound");
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
                            if (this.Logger.IsDebugEnabled)
                            {
                                this.Logger.DebugFormat("finished target {0} with edge {1} and fraction {2} with {3} cost",
                                    target, current.One, target.Fraction, current.Cost);
                            }

                            finishs[target] = current;

                            var edges = targetEdges[current.One];
                            edges.Remove(target);

                            if (edges.Count == 0)
                            {
                                targetEdges.Remove(current.One);
                            }
                            continue;
                        }
                    }
                }

                if (this.Logger.IsDebugEnabled)
                {
                    this.Logger.DebugFormat("succeed edge {0} with {1} cost", current.One.Id, current.Cost);
                }

                var successors = current.One.Successors;

                foreach (var successor in successors)
                {
                    var succcost = current.Cost + cost(successor);
                    var succbound = bound != null ? current.Bound + bound(successor) : 0.0;

                    if (targetEdges.ContainsKey(successor))
                    {
                        // reach target edge
                        foreach (var targetEdge in targetEdges[successor])
                        {
                            var reachcost = succcost - successor.Cost(1 - targetEdge.Fraction, cost);
                            var reachbound = bound != null ? succbound - successor.Cost(1 - targetEdge.Fraction, bound) : 0.0;    /// bound(successor, 1 - target.Fraction) : 0.0;
                            if (this.Logger.IsDebugEnabled)
                            {
                                this.Logger.DebugFormat("reached target {0} with successor edge {1} and fraction {2} with {3} cost",
                                    targetEdge, successor.Id, targetEdge.Fraction, reachcost);
                            }

                            var reach = new Mark(successor, current.One, reachcost, reachbound);
                            reaches.Add(reach, targetEdge);
                            priorities.Add(reach);
                        }
                    }

                    if (!entries.ContainsKey(successor))
                    {
                        if (this.Logger.IsDebugEnabled)
                        {
                            this.Logger.DebugFormat("added successor edge {0} with {1} cost", successor.Id, succcost);
                        }
                        Mark mark = new Mark(successor, current.One, succcost, succbound);
                        entries.Add(successor, mark);
                        priorities.Add(mark);
                    }
                }
            }

            var paths = new Dictionary<P, (P, IEnumerable<TEdge>)>();

            foreach (P targetPoint in targets)
            {
                if (finishs.ContainsKey(targetPoint))
                {
                    var path = new List<TEdge>();
                    var iterator = finishs.TryGetValue(targetPoint, out var outIterator) ? outIterator : null;
                    Mark start = null;
                    while (iterator != null)
                    {
                        path.Add(iterator.One);
                        start = iterator;
                        iterator = iterator.Two != null ? (entries.TryGetValue(iterator.Two, out var outIterator2) ? outIterator2 : null) : null;
                    }
                    path.Reverse();
                    var tp = (starts[start], path);
                    paths.Add(targetPoint, tp);
                }
            }

            entries.Clear();
            finishs.Clear();
            reaches.Clear();
            priorities.Clear();

            return paths;
        }
    }
}
