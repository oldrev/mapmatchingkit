using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    public readonly struct MatcherTransition
    {
        public Route Route { get; }

        public MatcherTransition(Route route)
        {
            this.Route = route;
        }
    }
}
