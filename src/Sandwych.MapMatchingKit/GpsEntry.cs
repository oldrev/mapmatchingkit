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
        public Point Point { get; }
        public DateTimeOffset Time { get; }

        public GpsEntry(in DateTimeOffset time, in Point point)
        {
            this.Time = time;
            this.Point = point;
        }

        public static TimeSpan TimeDelta(in GpsEntry prev, in GpsEntry next) =>
            next.Time - prev.Time;

        public double DistanceFrom(in GpsEntry other) =>
            DistanceOp.Distance(this.Point, other.Point);

    }
}
