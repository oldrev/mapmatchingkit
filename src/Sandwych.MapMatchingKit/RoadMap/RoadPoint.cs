using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.RoadMap
{
    public readonly struct RoadPoint : IEdgePoint<Road>
    {
        public Road Edge { get; }

        public double Fraction { get; }

        public IPoint Geometry { get; }

        public RoadPoint(in Road edge, in double fraction, IPoint point)
        {
            this.Edge = edge;
            this.Fraction = fraction;
            this.Geometry = point;
        }

    }
}
