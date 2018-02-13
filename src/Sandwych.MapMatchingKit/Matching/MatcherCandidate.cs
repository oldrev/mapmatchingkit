using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    public class MatcherCandidate :
        AbstractStateCandidate<MatcherCandidate, MatcherTransition, MatcherSample>
    {
        public RoadPoint RoadPoint { get; }

        public MatcherCandidate(RoadPoint roadPoint)
        {
            this.RoadPoint = roadPoint;
        }

        public override int GetHashCode() =>
            (this.RoadPoint, this.Predecessor, this.Transition, this.Filtprob, this.Seqprob).GetHashCode();
    }
}
