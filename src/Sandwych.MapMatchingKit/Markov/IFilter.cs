using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public interface IFilter<TCandidate, TTransition, TSample>
        where TCandidate : IStateCandidate<TCandidate, TTransition, TSample>
    {
        ISet<TCandidate> Execute(ISet<TCandidate> predecessors, in TSample previous, in TSample sample);
    }
}
