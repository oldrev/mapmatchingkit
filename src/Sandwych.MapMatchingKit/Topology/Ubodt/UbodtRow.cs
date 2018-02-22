using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public readonly struct UbodtRow
    {
        public long Source { get; }
        public long Target { get; }
        public long NextNode { get; }
        public long PrevNode { get; }
        public long NextEdge { get; }
        public double Cost { get; }
    }
}
