using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public readonly struct RoadPoint : IEdgePoint<Road>
    {
        public Road Edge { get; }

        public double Fraction { get; }

        public Coordinate2D Coordinate { get; }

        public float Azimuth { get; }

        public RoadPoint(in Road edge, double fraction, float azimuth, ISpatialOperation spatial)
        {
            this.Edge = edge;
            this.Fraction = fraction;
            this.Azimuth = azimuth;
            this.Coordinate = spatial.Interpolate(this.Edge.Geometry, this.Fraction); 
        }

        public RoadPoint(in Road edge, double fraction, float azimuth) : this(edge, fraction, azimuth, CartesianSpatialOperation.Instance)
        {
        }

        public RoadPoint(in Road edge, double fraction, ISpatialOperation spatial)
        {
            this.Edge = edge;
            this.Fraction = fraction;
            this.Azimuth = (float)spatial.Azimuth(edge.Geometry, fraction);
            this.Coordinate = spatial.Interpolate(this.Edge.Geometry, this.Fraction); 
        }

        public RoadPoint(in Road edge, double fraction) : this(edge, fraction, CartesianSpatialOperation.Instance)
        {

        }

        public override int GetHashCode() =>
            (this.Edge, this.Fraction, this.Coordinate, this.Azimuth).GetHashCode();

    }
}
