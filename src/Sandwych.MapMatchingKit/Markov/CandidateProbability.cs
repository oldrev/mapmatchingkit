using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public readonly struct CandidateProbability<TCandidate>
    {
        public TCandidate Candidate { get; }
        public double Probability { get; }

        public CandidateProbability(TCandidate cand, double prob)
        {
            this.Candidate = cand;
            this.Probability = prob;
        }
    }
}
