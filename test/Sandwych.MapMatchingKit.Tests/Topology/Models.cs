using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Tests.Topology
{
    public class Road : AbstractGraphEdge<Road>
    {
        public float Weight { get; }

        public Road(long id, long source, long target, float weight) : base(id, source, target)
        {
            this.Weight = weight;
        }

        public override int GetHashCode() =>
            (this.Id, this.Weight).GetHashCode();
    }

    public class Graph : AbstractGraph<Road>
    {
        public Graph(IEnumerable<Road> roads) : base(roads)
        {

        }
    }

    public readonly struct RoadPoint : IEdgePoint<Road>, IEquatable<RoadPoint>
    {
        public Road Edge { get; }
        public double Fraction { get; }

        public RoadPoint(Road road, double fraction)
        {
            this.Edge = road;
            this.Fraction = fraction;
        }

        public override int GetHashCode() =>
            (this.Edge, this.Fraction).GetHashCode();

        public bool Equals(RoadPoint other) =>
            this.Edge == other.Edge && Math.Abs(this.Fraction - other.Fraction) < 10E-6;
    }
}
