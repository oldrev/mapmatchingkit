using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandwych.MapMatchingKit.Matcher
{
    /// <summary>
    /// Matcher filter for Hidden Markov Model (HMM) map matching. It is a HMM filter (<see cref="IFilter{TCandidate, TTransition, TSample}"/>)
    /// and determines emission and transition probabilities for map matching with HMM.
    /// </summary>
    /// <typeparam name="TSampleId"></typeparam>
    public class Matcher<TSampleId> : AbstractFilter<MatcherCandidate<TSampleId>, MatcherTransition, MatcherSample<TSampleId>>
    {
        private readonly RoadMap _map;
        private readonly IGraphRouter<Road, RoadPoint> _router;
        private readonly ISpatialOperation _spatial;
        private readonly Func<Road, double> _cost;

        private double _sig2 = Math.Pow(5.0, 2.0);
        private double _sigA = Math.Pow(10.0, 2.0);
        private double _sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * Math.Pow(5.0, 2.0));
        private double _sqrt_2pi_sigA = Math.Sqrt(2d * Math.PI * Math.Pow(10.0, 2.0));


        /// <summary>
        /// Creates a HMM map matching filter for some map, router, cost function, and spatial operator.
        /// </summary>
        /// <param name="map">map <see cref="RoadMap" /> object of the map to be matched to.</param>
        /// <param name="router">router <see cref="IGraphRouter{TEdge, TPoint}"/> object to be used for route estimation.</param>
        /// <param name="cost">Cost function to be used for routing.</param>
        /// <param name="spatial">Spatial operator for spatial calculations.</param>
        public Matcher(RoadMap map, IGraphRouter<Road, RoadPoint> router, Func<Road, double> cost, ISpatialOperation spatial)
        {
            this._map = map;
            this._router = router;
            this._cost = cost;
            this._spatial = spatial;
        }

        /// <summary>
        /// Gets standard deviation in meters of gaussian distribution that defines emission probabilities.
        /// </summary>
        /// <returns>Standard deviation in meters of gaussian distribution that defines emission probabilities.</returns>
        public double GetSigma()
        {
            return Math.Sqrt(this._sig2);
        }


        /// <summary>
        /// Sets standard deviation in meters of gaussian distribution for defining emission
        /// probabilities (default is 5 meters).
        /// </summary>
        /// <param name="sigma">Standard deviation in meters of gaussian distribution for defining emission
        /// probabilities (default is 5 meters).
        /// </param>
        public void SetSigma(double sigma)
        {
            this._sig2 = Math.Pow(sigma, 2);
            this._sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * _sig2);
        }


        /// <summary>
        /// <para>
        /// Get or sets lambda parameter of negative exponential distribution defining transition probabilities
        /// (default is 0.0). It uses adaptive parameterization, if lambda is set to 0.0.
        /// </para>
        /// <para>Lambda parameter of negative exponential distribution defining transition probabilities.</para>
        /// </summary>
        public double Lambda { get; set; } = 0D;


        /// <summary>
        /// Gets or sets maximum radius for candidate selection in meters (default is 100 meters).
        /// </summary>
        public double MaxRadius { get; set; } = 100.0;


        /// <summary>
        /// Gets or sets maximum transition distance in meters (default is 15000 meters).
        /// </summary>
        public double MaxDistance { get; set; } = 15000.0;


        protected override (MatcherCandidate<TSampleId>, double)[] Candidates(
            ISet<MatcherCandidate<TSampleId>> predecessors, in MatcherSample<TSampleId> sample)
        {
            var points_ = this._map.Radius(sample.Coordinate, this.MaxRadius);
            var points = Minset.Minimize(points_);

            var map = new Dictionary<long, RoadPoint>();
            foreach (var point in points)
            {
                map.Add(point.Edge.Id, point);
            }

            /*
            foreach (var predecessor in predecessors)
            {
                var point = map.get(predecessor.RoadPoint.Edge.Id);
                if (point != null && point.edge() != null
                        && spatial.distance(point.geometry(),
                                predecessor.RoadPoint.Geometry) < getSigma()
                        && ((point.edge().heading() == Heading.forward
                                && point.fraction() < predecessor.point().fraction())
                                || (point.edge().heading() == Heading.backward
                                        && point.fraction() > predecessor.point().fraction())))
                {
                    points.Remove(point);
                    points.Add(predecessor.RoadPoint);
                }
            }
            */

            var candidates = new List<(MatcherCandidate<TSampleId>, double)>();

            foreach (var point in points)
            {
                double dz = _spatial.Distance(sample.Coordinate, point.Coordinate);
                double emission = 1 / _sqrt_2pi_sig2 * Math.Exp((-1) * dz * dz / (2 * _sig2));
                if (!double.IsNaN(sample.Azimuth))
                {
                    double da = sample.Azimuth > point.Azimuth
                            ? Math.Min(sample.Azimuth - point.Azimuth,
                                    360 - (sample.Azimuth - point.Azimuth))
                            : Math.Min(point.Azimuth - sample.Azimuth,
                                    360 - (point.Azimuth - sample.Azimuth));
                    emission *= Math.Max(1E-2, 1 / _sqrt_2pi_sigA * Math.Exp((-1) * da / (2 * _sigA)));
                }

                var candidate = new MatcherCandidate<TSampleId>(point);
                candidates.Add((candidate, emission));
            }

            return candidates.ToArray();
        }

        protected override (MatcherTransition, double) Transition(
                in (MatcherSample<TSampleId>, MatcherCandidate<TSampleId>) predecessor, in (MatcherSample<TSampleId>, MatcherCandidate<TSampleId>) candidate)
        {
            throw new NotSupportedException();
        }

        protected override IDictionary<MatcherCandidate<TSampleId>, IDictionary<MatcherCandidate<TSampleId>, (MatcherTransition, double)>> Transitions(
                in (MatcherSample<TSampleId>, ISet<MatcherCandidate<TSampleId>>) predecessors,
                in (MatcherSample<TSampleId>, ISet<MatcherCandidate<TSampleId>>) candidates)
        {

            var targets = new HashSet<RoadPoint>();
            foreach (var candidate in candidates.Item2)
            {
                targets.Add(candidate.RoadPoint);
            }

            var transitions = new Dictionary<MatcherCandidate<TSampleId>, IDictionary<MatcherCandidate<TSampleId>, (MatcherTransition, double)>>();
            var base_ = 1.0 * _spatial.Distance(predecessors.Item1.Coordinate, candidates.Item1.Coordinate) / 60.0;
            var bound = Math.Max(1000.0, Math.Min(this.MaxDistance, ((candidates.Item1.Time - predecessors.Item1.Time) / 1000.0) * 100.0));

            foreach (var predecessor in predecessors.Item2)
            {
                var map = new Dictionary<MatcherCandidate<TSampleId>, (MatcherTransition, double)>();
                //TODO check return
                var routes = _router.Route(predecessor.RoadPoint, targets, _cost, Costs.DistanceCost, bound);

                foreach (var candidate in candidates.Item2)
                {
                    if (routes.TryGetValue(candidate.RoadPoint, out var edges))
                    {
                        var route = new Route(predecessor.RoadPoint, candidate.RoadPoint, edges);

                        // According to Newson and Krumm 2009, transition probability is lambda *
                        // Math.exp((-1.0) * lambda * Math.abs(dt - route.length())), however, we
                        // experimentally choose lambda * Math.exp((-1.0) * lambda * Math.max(0,
                        // route.length() - dt)) to avoid unnecessary routes in case of u-turns.

                        var beta = this.Lambda == 0D
                                ? (2.0 * Math.Max(1d,
                                        candidates.Item1.Time - predecessors.Item1.Time) / 1000D)
                                : 1D / this.Lambda;

                        var transition = (1D / beta) * Math.Exp(
                                (-1.0) * Math.Max(0D, route.Cost(_cost) - base_) / beta);

                        map.Add(candidate, (new MatcherTransition(route), transition));
                    }
                }

                transitions.Add(predecessor, map);
            }

            return transitions;
        }
    }
}
