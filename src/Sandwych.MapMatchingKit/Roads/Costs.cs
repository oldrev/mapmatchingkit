using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Roads
{
    public static class Costs
    {
        private const double HeuristicSpeed = 130.0;
        private const double HuristicPriority = 1.0;

        public static double DistanceCost(Road road) => road.Length;

        public static double TimeCost(Road road) => DistanceCost(road) * 3.6 / Math.Min(road.MaxSpeed, HeuristicSpeed);

        public static double TimePriorityCost(Road road) => TimeCost(road) * Math.Max(HuristicPriority, road.Priority);

        public static double ComputeCost<TEdge>(this TEdge edge, double fraction, Func<TEdge, double> costFunc)
            where TEdge : IGraphEdge<TEdge>
            => costFunc(edge) * fraction;
    }

}
