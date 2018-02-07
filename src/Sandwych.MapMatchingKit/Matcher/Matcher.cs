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
        private readonly RoadMap map;
        private readonly IGraphRouter<Road, RoadPoint> router;
        private readonly ISpatialService spatial;
        private readonly Func<Road, double> cost;

        private double sig2 = Math.Pow(5.0, 2.0);
        private double sigA = Math.Pow(10.0, 2.0);
        private double sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * Math.Pow(5.0, 2.0));
        private double sqrt_2pi_sigA = Math.Sqrt(2d * Math.PI * Math.Pow(10.0, 2.0));
        private double lambda = 0d;
        private double radius = 200;
        private double distance = 15000;


        /// <summary>
        /// Creates a HMM map matching filter for some map, router, cost function, and spatial operator.
        /// </summary>
        /// <param name="map">map <see cref="RoadMap" /> object of the map to be matched to.</param>
        /// <param name="router">router <see cref="IGraphRouter{TEdge, TPoint}"/> object to be used for route estimation.</param>
        /// <param name="cost">Cost function to be used for routing.</param>
        /// <param name="spatial">Spatial operator for spatial calculations.</param>
        public Matcher(RoadMap map, IGraphRouter<Road, RoadPoint> router, Func<Road, double> cost, ISpatialService spatial)
        {
            this.map = map;
            this.router = router;
            this.cost = cost;
            this.spatial = spatial;
        }


        /*
         *
         */
        /// <summary>
        /// Gets standard deviation in meters of gaussian distribution that defines emission probabilities.
        /// </summary>
        /// <returns>Standard deviation in meters of gaussian distribution that defines emission probabilities.</returns>
        public double getSigma()
        {
            return Math.Sqrt(this.sig2);
        }


        /// <summary>
        /// Sets standard deviation in meters of gaussian distribution for defining emission
        /// probabilities (default is 5 meters).
        /// </summary>
        /// <param name="sigma">Standard deviation in meters of gaussian distribution for defining emission
        /// probabilities (default is 5 meters).
        /// </param>
        public void setSigma(double sigma)
        {
            this.sig2 = Math.Pow(sigma, 2);
            this.sqrt_2pi_sig2 = Math.Sqrt(2d * Math.PI * sig2);
        }


        /// <summary>
        /// Gets lambda parameter of negative exponential distribution defining transition probabilities.
        /// </summary>
        /// <returns>Lambda parameter of negative exponential distribution defining transition probabilities.</returns>
        public double getLambda()
        {
            return this.lambda;
        }

        /**
         * Sets lambda parameter of negative exponential distribution defining transition probabilities
         * (default is 0.0). It uses adaptive parameterization, if lambda is set to 0.0.
         *
         * @param lambda Lambda parameter of negative exponential distribution defining transition
         *        probabilities.
         */
        public void setLambda(double lambda)
        {
            this.lambda = lambda;
        }

        /**
         * Gets maximum radius for candidate selection in meters.
         *
         * @return Maximum radius for candidate selection in meters.
         */
        public double getMaxRadius()
        {
            return this.radius;
        }

        /**
         * Sets maximum radius for candidate selection in meters (default is 100 meters).
         *
         * @param radius Maximum radius for candidate selection in meters.
         */
        public void setMaxRadius(double radius)
        {
            this.radius = radius;
        }

        /**
         * Gets maximum transition distance in meters.
         *
         * @return Maximum transition distance in meters.
         */
        public double getMaxDistance() => this.distance;

        /**
         * Sets maximum transition distance in meters (default is 15000 meters).
         *
         * @param distance Maximum transition distance in meters.
         */
        public void setMaxDistance(double distance)
        {
            this.distance = distance;
        }

        protected override (MatcherCandidate<TSampleId>, double)[] Candidates(
            ISet<MatcherCandidate<TSampleId>> predecessors, in MatcherSample<TSampleId> sample)
        {
            var points_ = this.map.Radius(sample.Coordinate, radius);
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
                double dz = spatial.Distance(sample.Coordinate, point.Coordinate);
                double emission = 1 / sqrt_2pi_sig2 * Math.Exp((-1) * dz * dz / (2 * sig2));
                if (!double.IsNaN(sample.Azimuth))
                {
                    double da = sample.Azimuth > point.Azimuth
                            ? Math.Min(sample.Azimuth - point.Azimuth,
                                    360 - (sample.Azimuth - point.Azimuth))
                            : Math.Min(point.Azimuth - sample.Azimuth,
                                    360 - (point.Azimuth - sample.Azimuth));
                    emission *= Math.Max(1E-2, 1 / sqrt_2pi_sigA * Math.Exp((-1) * da / (2 * sigA)));
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
            var base_ = 1.0 * spatial.Distance(predecessors.Item1.Coordinate, candidates.Item1.Coordinate) / 60.0;
            var bound = Math.Max(1000.0, Math.Min(distance, ((candidates.Item1.Time - predecessors.Item1.Time) / 1000.0) * 100.0));

            foreach (var predecessor in predecessors.Item2)
            {
                var map = new Dictionary<MatcherCandidate<TSampleId>, (MatcherTransition, double)>();
                //TODO check return
                router.TryRoute(predecessor.RoadPoint, targets, cost, Costs.DistanceCost, bound, out var routes);

                foreach (var candidate in candidates.Item2)
                {
                    var edges = routes[candidate.RoadPoint];

                    if (edges == null)
                    {
                        continue;
                    }

                    var route = new Route(predecessor.RoadPoint, candidate.RoadPoint, edges);

                    // According to Newson and Krumm 2009, transition probability is lambda *
                    // Math.exp((-1.0) * lambda * Math.abs(dt - route.length())), however, we
                    // experimentally choose lambda * Math.exp((-1.0) * lambda * Math.max(0,
                    // route.length() - dt)) to avoid unnecessary routes in case of u-turns.

                    double beta = lambda == 0
                            ? (2.0 * Math.Max(1d,
                                    candidates.Item1.Time - predecessors.Item1.Time) / 1000)
                            : 1 / lambda;

                    double transition = (1 / beta) * Math.Exp(
                            (-1.0) * Math.Max(0, route.Cost(cost) - base_) / beta);

                    map.Add(candidate, (new MatcherTransition(route), transition));
                }

                transitions.Add(predecessor, map);
            }

            return transitions;
        }
    }
}
