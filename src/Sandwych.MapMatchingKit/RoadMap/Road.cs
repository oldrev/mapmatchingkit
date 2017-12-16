using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.RoadMap
{
    public class Road : AbstractGraphEdge
    {
        public double Length { get; }
        public ILineString Geometry { get; }

        public Road(int id, int source, int target, double length, ILineString geometry) : base(id, source, target)
        {
            this.Length = length;
            this.Geometry = geometry;
        }

    }
}
