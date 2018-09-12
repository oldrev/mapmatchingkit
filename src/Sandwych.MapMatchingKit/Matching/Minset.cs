using Sandwych.MapMatchingKit.Roads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{

    /// <summary>
    /// Minimizes a set of matching candidates represented as <see cref="RoadPoint"/> to remove semantically
    /// redundant candidates.
    /// </summary>
    public static class Minset
    {
        /// <summary>
        /// Floating point precision for considering a {@link RoadPoint} be the same as a vertex,
        /// fraction is zero or one (default: 1E-8).
        /// </summary>
        public readonly static double Precision = 1E-8;

        private static double Round(double value)
        {
            return Math.Round(value / Precision) * Precision;
        }

        /// <summary>
        /// <para>
        /// Removes semantically redundant matching candidates from a set of matching candidates (as
        ///  <see cref="RoadPoint"/> object) and returns a minimized (reduced) subset.
        /// </para>
        /// <para>
        /// Given a position measurement, a matching candidate is each road in a certain radius of the
        /// measured position, and in particular that point on each road that is closest to the measured
        /// position. Hence, there are as many state candidates as roads in that area. The idea is to
        /// conserve only possible routes through the area and use each route with its closest point to
        /// the measured position as a matching candidate. Since roads are split into multiple segments,
        /// the number of matching candidates is significantly higher than the respective number of
        /// routes. To give an example, assume the following matching candidates as <see cref="RoadPoint"/> 
        /// objects with a road id and a fraction:
        /// </para>
        ///
        /// <ul>
        /// <li><i>(r<sub>i</sub>, 0.5)</i>
        /// <li><i>(r<sub>j</sub>, 0.0)</i>
        /// <li><i>(r<sub>k</sub>, 0.0)</i>
        /// </ul>
        ///
        /// <para>
        /// where they are connected as <i>r<sub>i</sub> &#8594; r<sub>j</sub></i> and <i>r<sub>i</sub>
        /// &#8594; r<sub>k</sub></i>. Here, matching candidates <i>r<sub>j</sub></i> and
        /// <i>r<sub>k</sub></i> can be removed if we see routes as matching candidates. This is because
        /// both, <i>r<sub>j</sub></i> and <i>r<sub>k</sub></i>, are reachable from <i>r<sub>i</sub></i>.
        /// </para>
        /// <para>
        /// <b>Note:</b> Of course, <i>r<sub>j</sub></i> and <i>r<sub>k</sub></i> may be seen as relevant
        /// matching candidates, however, in the present HMM map matching algorithm there is no
        /// optimization of matching candidates along the road, instead it only considers the closest
        /// point of a road as a matching candidate.
        /// </para>
        /// </summary>
        /// <param name="candidates">candidates Set of matching candidates as <see cref="RoadPoint"> objects.</param>
        /// <returns>Minimized (reduced) set of matching candidates as <see cref="RoadPoint"/> objects</returns>
        public static HashSet<RoadPoint> Minimize(IEnumerable<RoadPoint> candidates)
        {
            var map = new Dictionary<long, RoadPoint>(candidates.Count());
            var misses = new Dictionary<long, int>(candidates.Count());
            var removes = new List<long>(candidates.Count());

            foreach (var candidate in candidates)
            {
                map[candidate.Edge.Id] = candidate;
                misses[candidate.Edge.Id] = 0;
            }

            foreach (var candidate in candidates)
            {
                var successors = candidate.Edge.Successors;
                var id = candidate.Edge.Id;

                foreach (var successor in successors)
                {
                    var mapContainsSuccessorId = map.ContainsKey(successor.Id);
                    if (!mapContainsSuccessorId)
                    {
                        misses[id] = misses[id] + 1;
                    }
                    if (mapContainsSuccessorId && Round(map[successor.Id].Fraction) == 0)
                    {
                        removes.Add(successor.Id);
                        misses[id] = misses[id] + 1;
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                var id = candidate.Edge.Id;
                if (map.ContainsKey(id) && !removes.Contains(id) && Round(candidate.Fraction) == 1 && misses[id] == 0)
                {
                    removes.Add(id);
                }
            }

            foreach (var id in removes)
            {
                map.Remove(id);
            }

            return new HashSet<RoadPoint>(map.Values);
        }
    }
}
