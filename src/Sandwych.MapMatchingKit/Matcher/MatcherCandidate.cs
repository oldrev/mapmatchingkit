using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matcher
{
    public class MatcherCandidate<TSampleId> :
        AbstractStateCandidate<MatcherCandidate<TSampleId>, MatcherTransition, MatcherSample<TSampleId>>
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
