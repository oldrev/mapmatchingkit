using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    using CandidateProbability = CandidateProbability<MatcherCandidate>;
    using TransitionProbability = TransitionProbability<MatcherTransition>;


    /// <summary>
    /// Matcher filter for Hidden Markov Model (HMM) map matching. It is a HMM filter (<see cref="IFilter{TCandidate, TTransition, TSample}"/>)
    /// and determines emission and transition probabilities for map matching with HMM.
    /// </summary>
    /// <typeparam name="TCandidate">Candidate inherits from {@link StateCandidate}.</typeparam>
    /// <typeparam name="TTransition">Transition inherits from {@link StateTransition}.</typeparam>
    /// <typeparam name="TSample">Sample inherits from {@link Sample}.</typeparam>
    public class Matcher<TCandidate, TTransition, TSample> : AbstractFilter<MatcherCandidate, MatcherTransition, MatcherSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
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
        /// Gets or sets standard deviation in meters of gaussian distribution for defining emission
        /// probabilities (default is 5 meters).
        /// </summary>
        public double Sigma
        {
            get => Math.Sqrt(this._sig2);
            set
            {
                this._sig2 = Math.Pow(value, 2);
                this._sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * _sig2);
            }
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


        public override IReadOnlyCollection<CandidateProbability> ComputeCandidates(
            IEnumerable<MatcherCandidate> predecessors, in MatcherSample sample)
        {
            var points_ = this._map.Radius(sample.Coordinate, this.MaxRadius);
            var points = Minset.Minimize(points_);

            var dict = new Dictionary<long, RoadPoint>(points.Count);
            foreach (var point in points)
            {
                dict.Add(point.Edge.Id, point);
            }

            foreach (var predecessor in predecessors)
            {
                var pointExisted = dict.TryGetValue(predecessor.Point.Edge.Id, out var point);
                if (pointExisted && point.Edge != null
                        && _spatial.Distance(point.Coordinate, predecessor.Point.Coordinate) < this.Sigma
                        && ((point.Edge.Headeing == Heading.Forward
                                && point.Fraction < predecessor.Point.Fraction)
                                || (point.Edge.Headeing == Heading.Backward
                                        && point.Fraction > predecessor.Point.Fraction)))
                {
                    points.Remove(point);
                    points.Add(predecessor.Point);
                }
            }

            var candidates = new List<CandidateProbability>(points.Count);

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

                var candidate = new MatcherCandidate(sample, point);
                candidates.Add(new CandidateProbability(candidate, emission));
            }

            return candidates;
        }

        public override TransitionProbability ComputeTransition(
                in (MatcherSample, MatcherCandidate) predecessor, in (MatcherSample, MatcherCandidate) candidate)
        {
            throw new NotSupportedException();
        }

        public override IDictionary<MatcherCandidate, IDictionary<MatcherCandidate, TransitionProbability>> ComputeTransitions(
                in (MatcherSample, IEnumerable<MatcherCandidate>) predecessors,
                in (MatcherSample, IEnumerable<MatcherCandidate>) candidates)
        {
            var targets = candidates.Item2.Select(c => c.Point);

            var transitions = new Dictionary<MatcherCandidate, IDictionary<MatcherCandidate, TransitionProbability>>();
            var base_ = 1.0 * _spatial.Distance(predecessors.Item1.Coordinate, candidates.Item1.Coordinate) / 60.0;
            var bound = Math.Max(1000.0, Math.Min(this.MaxDistance, (candidates.Item1.Time - predecessors.Item1.Time).TotalSeconds * 100.0));

            foreach (var predecessor in predecessors.Item2)
            {
                var map = new Dictionary<MatcherCandidate, TransitionProbability>();
                //TODO check return
                var routes = _router.Route(predecessor.Point, targets, _cost, Costs.DistanceCost, bound);

                foreach (var candidate in candidates.Item2)
                {
                    if (routes.TryGetValue(candidate.Point, out var edges))
                    {
                        var route = new Route(predecessor.Point, candidate.Point, edges);

                        // According to Newson and Krumm 2009, transition probability is lambda *
                        // Math.exp((-1.0) * lambda * Math.abs(dt - route.length())), however, we
                        // experimentally choose lambda * Math.exp((-1.0) * lambda * Math.max(0,
                        // route.length() - dt)) to avoid unnecessary routes in case of u-turns.

                        var beta = this.Lambda == 0D
                                ? (2.0 * Math.Max(1d, (candidates.Item1.Time - predecessors.Item1.Time).TotalMilliseconds) / 1000D)
                                : 1D / this.Lambda;

                        var transition = (1D / beta) * Math.Exp(
                                (-1.0) * Math.Max(0D, route.Cost(_cost) - base_) / beta);

                        map.Add(candidate, new TransitionProbability(new MatcherTransition(route), transition));
                    }
                }

                transitions.Add(predecessor, map);
            }

            return transitions;
        }
    }
}
