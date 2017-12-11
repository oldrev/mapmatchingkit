using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using Sandwych.MapMatchingKit.Geometry;

namespace Sandwych.MapMatchingKit
{
    public readonly struct GpsEntry
    {
        private readonly Point _point;
        private readonly DateTimeOffset _time;

        public Point Point => _point;
        public DateTimeOffset Time => _time;

        public GpsEntry(in DateTimeOffset time, in Point point)
        {
            _time = time;
            _point = point;
        }

        public TimeSpan TimeDelta(GpsEntry prev, GpsEntry next) =>
            next.Time - prev.Time;

        public double DistanceFrom(GpsEntry other) =>
            DistanceOp.Distance(this.Point, other.Point);

    }
}
