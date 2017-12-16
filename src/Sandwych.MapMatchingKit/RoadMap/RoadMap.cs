using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.RoadMap
{
    public class RoadMap : AbstractGraph<Road>
    {
        public RoadMap(IEnumerable<Road> roads) : base(roads)
        {

        }
    }
}
