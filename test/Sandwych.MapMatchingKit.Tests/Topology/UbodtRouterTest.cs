using System;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Topology;

namespace Sandwych.MapMatchingKit.Tests.Topology
{
    public class UbodtRouterTest : AbstractRouterTest
    {
        protected override IGraphRouter<Road, RoadPoint> CreateRouter() => new UbodtRouter<Road, RoadPoint>();
    }
}
