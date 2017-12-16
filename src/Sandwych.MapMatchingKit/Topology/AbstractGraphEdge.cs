using System;
using System.Collections.Generic;
using System.Text;
using QuickGraph;

namespace Sandwych.MapMatchingKit.Topology
{
    public abstract class AbstractGraphEdge : IGraphEdge
    {
        public int Id { get; }

        public int Source { get; }

        public int Target { get; }

        public AbstractGraphEdge(int id, int source, int target)
        {
            this.Id = id;
            this.Source = source;
            this.Target = target;
        }
    }
}
