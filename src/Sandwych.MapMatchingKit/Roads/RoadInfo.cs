using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Roads
{
    public class RoadInfo
    {
        public ILineString Geometry { get; }
        public long Id { get; }
        public byte[] ToWkb() => this.Geometry.AsBinary();
        public string ToWkt() => this.Geometry.AsText();
        public long Source { get; }
        public long Target { get; }
        public bool OneWay { get; }
        public short Type { get; }
        public float Priority { get; }
        public float MaxSpeedForward { get; }
        public float MaxSpeedBackward { get; }
        public float Length { get; }

        public RoadInfo(long id, long source, long target, bool oneway, short type,
                float priority, float maxspeedForward, float maxspeedBackward, float length,
                ILineString geometry)
        {
            this.Id = id;
            this.Source = source;
            this.Target = target;
            this.OneWay = oneway;
            this.Type = type;
            this.Priority = priority;
            this.MaxSpeedForward = maxspeedForward;
            this.MaxSpeedBackward = maxspeedBackward;
            this.Length = length;
            this.Geometry = geometry;
        }

    }
}
