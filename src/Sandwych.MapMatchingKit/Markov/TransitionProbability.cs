using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public readonly struct TransitionProbability<TTransition>
    {
        public TTransition Transition { get; }
        public double Probability { get; }

        public TransitionProbability(TTransition transition, double prob)
        {
            this.Transition = transition;
            this.Probability = prob;
        }
    }
}
