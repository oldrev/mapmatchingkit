using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public class Road : AbstractGraphEdge<Road>
    {
        public RoadInfo RoadInfo { get; }
        public Heading Headeing { get; }
        public ILineString Geometry { get; }

        public Road(RoadInfo info, Heading heading) :
            base(heading == Heading.Forward ? info.Id * 2 : info.Id * 2 + 1,
                heading == Heading.Forward ? info.Source : info.Target,
                heading == Heading.Forward ? info.Target : info.Source)
        {
            this.RoadInfo = info;
            this.Headeing = heading;
            if (heading == Heading.Forward)
            {
                this.Geometry = info.Geometry;
            }
            else
            {
                this.Geometry = info.Geometry.Reverse() as ILineString;
            }
        }

        public float Length => this.RoadInfo.Length;
        public float MaxSpeed => this.Headeing == Heading.Forward ? this.RoadInfo.MaxSpeedForward : this.RoadInfo.MaxSpeedBackward;
        public float Priority => this.RoadInfo.Priority;
    }
}
