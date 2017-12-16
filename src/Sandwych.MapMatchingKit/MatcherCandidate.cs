using System;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Roads;

namespace Sandwych.MapMatchingKit
{
    public readonly struct MatcherCandidate
    {
        public bool IsDirected => false;
        public TrajectoryEntry Entry { get; }
        public RoadPoint RoadPoint { get; }

        public MatcherCandidate(in TrajectoryEntry entry, in RoadPoint roadPoint)
        {
            this.Entry = entry;
            this.RoadPoint = roadPoint;
        }

    }
}
