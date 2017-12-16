using System;
using System.Linq;
using Sandwych.MapMatchingKit.Topology;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Spatial;

namespace Sandwych.MapMatchingKit.Roads
{
    public class RoadMap : AbstractGraph<Road>
    {
        public ISpatialIndex Index { get; }

        public RoadMap(IEnumerable<Road> roads) : base(roads)
        {
            var indexItems = roads.Select(r => new SpatialIndexItem(r.Id, r.Geometry));

            this.Index = new QuadtreeIndex(indexItems);
        }
    }
}
