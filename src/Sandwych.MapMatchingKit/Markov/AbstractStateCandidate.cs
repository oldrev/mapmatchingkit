using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public abstract class AbstractStateCandidate<TCandidate, TTransition, TSample> :
        IStateCandidate<TCandidate, TTransition, TSample>
        where TCandidate : IStateCandidate<TCandidate, TTransition, TSample>
    {
        private TTransition _transition;

        public double Seqprob { get; set; }
        public double Filtprob { get; set; }
        public TCandidate Predecessor { get; set; }
        public bool HasTransition { get; private set; }
        public TSample Sample { get; }

        protected AbstractStateCandidate(in TSample sample)
        {
            this.Sample = sample;
        }

        public TTransition Transition
        {
            get
            {
                if (!this.HasTransition)
                {
                    throw new InvalidOperationException();
                }
                return _transition;
            }
            set
            {
                _transition = value;
                this.HasTransition = true;
            }
        }

        public virtual bool Equals(TCandidate other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return Predecessor.Equals(other.Predecessor) && Transition.Equals(other.Transition);
        }
    }
}
