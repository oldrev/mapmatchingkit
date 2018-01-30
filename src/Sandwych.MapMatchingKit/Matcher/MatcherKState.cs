using System;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Markov;

namespace Sandwych.MapMatchingKit.Matcher
{
    public class MatcherKState<TSampleId> :
        KState<MatcherCandidate<TSampleId>, MatcherTransition, MatcherSample<TSampleId>>
    {
    }
}
