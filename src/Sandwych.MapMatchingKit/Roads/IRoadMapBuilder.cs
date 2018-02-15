using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Roads
{
    public interface IRoadMapBuilder
    {
        IRoadMapBuilder AddRoad(RoadInfo road);
        IRoadMapBuilder AddRoads(IEnumerable<RoadInfo> roads);
        RoadMap Build();
    }
}
