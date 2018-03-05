using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public interface IFilter<TCandidate, TTransition, TSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
    {
        ICollection<TCandidate> Execute(IEnumerable<TCandidate> predecessors, in TSample previous, in TSample sample);
    }
}
