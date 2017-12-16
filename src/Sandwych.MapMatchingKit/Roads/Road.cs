using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public class Road : AbstractGraphEdge
    {
        public double Length { get; }
        public IMultiLineString Geometry { get; }

        public Road(int id, int source, int target, double length, ILineString geometry) : base(id, source, target)
        {
            this.Length = length;
            this.Geometry = new MultiLineString(new ILineString[] { geometry });
        }

        public Road(int id, int source, int target, double length, IMultiLineString geometry) : base(id, source, target)
        {
            this.Length = length;
            this.Geometry = geometry;
        }

    }
}
