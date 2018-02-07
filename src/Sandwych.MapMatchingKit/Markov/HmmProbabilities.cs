/**
 * Copyright (C) 2015-2016, BMW Car IT GmbH and BMW AG
 * Author: Stefan Holder (stefan.holder@bmw.de)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    /// <summary>
    /// Based on Newson, Paul, and John Krumm. "Hidden Markov map matching through
    /// noise and sparseness." Proceedings of the 17th ACM SIGSPATIAL International
    /// Conference on Advances in Geographic Information Systems. ACM, 2009.
    /// </summary>
    public class HmmProbabilities
    {

        public double Sigma { get; }
        public double Beta { get; }

        /// <summary>
        /// Sets default values for sigma and beta.
        /// </summary>
        public HmmProbabilities() : this(4.07, 0.00959442)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sigma">standard deviation of the normal distribution [m] used for modeling the GPS error</param>
        /// <param name="beta">beta parameter of the exponential distribution used for modeling transition probabilities</param>
        public HmmProbabilities(in double sigma, in double beta)
        {
            Sigma = sigma;
            Beta = beta;
        }

        /// <summary>
        /// Returns the logarithmic emission probability density.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>Absolute distance [m] between GPS measurement and map matching candidate.</returns>
        public double EmissionLogProbability(in double distance)
        {
            return Distributions.LogNormalDistribution(this.Sigma, distance);
        }

        /// <summary>
        /// Returns the logarithmic transition probability density for the given
        /// transition parameters.
        /// </summary>
        /// <param name="routeLength"> Length of the shortest route [m] between two consecutive map matching candidates.</param>
        /// <param name="linearDistance">Linear distance [m] between two consecutive GPS measurements.</param>
        /// <returns></returns>
        public double TransitionLogProbability(in double routeLength, in double linearDistance)
        {
            // Transition metric taken from Newson & Krumm.
            var transitionMetric = Math.Abs(linearDistance - routeLength);

            return Distributions.LogExponentialDistribution(this.Beta, transitionMetric);
        }


        /// <summary>
        /// Returns the logarithmic transition probability density for the given transition
        /// parameters.
        /// </summary>
        /// <param name="routeLength">Length of the shortest route [m] between two consecutive map matching candidates.</param>
        /// <param name="linearDistance">Linear distance [m] between two consecutive GPS measurements.</param>
        /// <param name="timeDiff">time difference [s] between two consecutive GPS measurements.</param>
        /// <returns></returns>
        public double TransitionLogProbability(double routeLength, double linearDistance, double timeDiff)
        {
            var transitionMetric = NormalizedTransitionMetric(routeLength, linearDistance, timeDiff);
            return Distributions.LogExponentialDistribution(this.Beta, transitionMetric);
        }


        /// <summary>
        /// Returns a transition metric for the transition between two consecutive map matching
        /// candidates.
        ///
        /// In contrast to Newson & Krumm the absolute distance difference is divided by the quadratic
        /// time difference to make the beta parameter of the exponential distribution independent of the
        /// sampling interval.
        /// </summary>
        /// <param name="routeLength"></param>
        /// <param name="linearDistance"></param>
        /// <param name="timeDiff"></param>
        /// <returns></returns>
        private double NormalizedTransitionMetric(double routeLength, double linearDistance,
                double timeDiff)
        {
            if (timeDiff < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeDiff),
                        "Time difference between subsequent location measurements must be >= 0.");
            }
            return Math.Abs(linearDistance - routeLength) / (timeDiff * timeDiff);
        }


    }
}
