using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit
{
    public readonly struct MatcherCandidate
    {
        public bool IsDirected => false;
        public TrajectoryEntry Entry { get; }

        public MatcherCandidate(in TrajectoryEntry entry)
        {
            this.Entry = entry;
        }

    }
}
