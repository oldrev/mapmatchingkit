using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    public class MatcherTransition
    {
        public Route Route { get; }

        public MatcherTransition(Route route)
        {
            this.Route = route ?? throw new ArgumentNullException(nameof(route));
        }

        public override int GetHashCode() =>
            this.Route.GetHashCode();
    }
}
