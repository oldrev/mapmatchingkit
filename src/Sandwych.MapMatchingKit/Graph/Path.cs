using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.EdgeGraph;

namespace Sandwych.MapMatchingKit.Graph
{
    public class Path
    {
        public EdgeGraph Graph { get; }

        public Path(EdgeGraph graph)
        {
            this.Graph = graph;
        }

    }
}
