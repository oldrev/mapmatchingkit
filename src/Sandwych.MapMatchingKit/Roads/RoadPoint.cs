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

        public RoadPoint(in Road edge, double fraction, float azimuth)
        {
            this.Edge = edge;
            this.Fraction = fraction;
            this.Azimuth = azimuth;
            this.Coordinate = CartesianSpatialService.Instance.Interpolate(this.Edge.Geometry, this.Fraction); //TODO 
        }

        public RoadPoint(in Road edge, double fraction)
        {
            this.Edge = edge;
            this.Fraction = fraction;
            this.Azimuth = (float)CartesianSpatialService.Instance.Azimuth(edge.Geometry, fraction);
            this.Coordinate = CartesianSpatialService.Instance.Interpolate(this.Edge.Geometry, this.Fraction); //TODO 
        }

        public static RoadPoint FromRoadFraction(in Road edge, double fraction, ISpatialService spatial)
        {
            var azimuth = spatial.Azimuth(edge.Geometry, fraction);
            return new RoadPoint(edge, fraction, (float)azimuth);
        }

        public override int GetHashCode() =>
            (this.Edge, this.Fraction, this.Coordinate, this.Azimuth).GetHashCode();

    }
}
