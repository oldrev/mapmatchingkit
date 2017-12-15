using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace Sandwych.MapMatchingKit
{
    public readonly struct TrajectoryEntry
    {
        public long Id { get; }
        public Point Point { get; }
        public DateTimeOffset Time { get; }

        public TrajectoryEntry(in long id, in DateTimeOffset time, in Point point)
        {
            this.Id = id;
            this.Time = time;
            this.Point = point;
        }

        public static TimeSpan TimeDelta(in TrajectoryEntry prev, in TrajectoryEntry next) =>
            next.Time - prev.Time;

        public double DistanceFrom(in TrajectoryEntry other) =>
            DistanceOp.Distance(this.Point, other.Point);

    }
}
