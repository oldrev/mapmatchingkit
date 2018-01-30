using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public abstract class AbstractStateCandidate<TCandidate, TTransition, TSample> :
        IStateCandidate<TCandidate, TTransition, TSample>
        where TCandidate : IStateCandidate<TCandidate, TTransition, TSample>
    {
        public double Seqprob { get; set; }
        public double Filtprob { get; set; }
        public TTransition Transition { get; set; }
        public TCandidate Predecessor { get; set; }

        public AbstractStateCandidate()
        {
        }
    }
}
