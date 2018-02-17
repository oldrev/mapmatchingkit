using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public readonly struct RoadPoint : IEdgePoint<Road>, IEquatable<RoadPoint>
    {
        public Road Edge { get; }

        public double Fraction { get; }

        public Coordinate2D Coordinate { get; }

        public float Azimuth { get; }

        public RoadPoint(in Road road, double fraction, float azimuth, ISpatialOperation spatial)
        {
            this.Edge = road;
            this.Fraction = fraction;
            this.Azimuth = azimuth;
            this.Coordinate = spatial.Interpolate(this.Edge.Geometry, this.Fraction);
        }

        public RoadPoint(in Road road, double fraction, float azimuth) : this(road, fraction, azimuth, GeographySpatialOperation.Instance)
        {
        }

        public RoadPoint(in Road road, double fraction, ISpatialOperation spatial)
        {
            this.Edge = road;
            this.Fraction = fraction;
            this.Azimuth = (float)spatial.Azimuth(road.Geometry, fraction);
            this.Coordinate = spatial.Interpolate(this.Edge.Geometry, this.Fraction);
        }

        public RoadPoint(in Road road, double fraction) : this(road, fraction, GeographySpatialOperation.Instance)
        {

        }

        public override int GetHashCode() =>
            (this.Edge, this.Fraction, this.Coordinate, this.Azimuth).GetHashCode();


        public bool Equals(RoadPoint other)
        {
            return object.ReferenceEquals(this.Edge, other.Edge) && this.Fraction == other.Fraction && this.Coordinate == other.Coordinate && this.Azimuth == other.Azimuth;
        }
    }
}
