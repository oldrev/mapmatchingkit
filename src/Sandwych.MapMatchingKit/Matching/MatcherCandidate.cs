using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    public sealed class MatcherCandidate : IStateCandidate<MatcherCandidate, MatcherTransition, MatcherSample>
    {
        private readonly RoadPoint _point;

        public ref readonly RoadPoint Point => ref _point;

        public double Seqprob { get; set; }
        public double Filtprob { get; set; }
        public MatcherCandidate Predecessor { get; set; }
        public MatcherTransition Transition { get; set; }

        public MatcherCandidate(in RoadPoint point)
        {
            this._point = point;
        }

        public override int GetHashCode() =>
            (this.Point, this.Predecessor, this.Transition, this.Filtprob, this.Seqprob).GetHashCode();

        public bool Equals(MatcherCandidate other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Point.Equals(other.Point) && this.Predecessor.Equals(other.Predecessor) && this.Transition.Equals(other.Transition);
        }
    }
}
