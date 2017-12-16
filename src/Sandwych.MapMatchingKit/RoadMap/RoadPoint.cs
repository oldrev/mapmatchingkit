using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.RoadMap
{
    public readonly struct RoadPoint : IEdgePoint<Road>
    {
        public Road Edge { get; }

        public double Fraction { get; }

        public RoadPoint(in Road edge, in double fraction)
        {
            this.Edge = edge;
            this.Fraction = fraction;
        }

    }
}
