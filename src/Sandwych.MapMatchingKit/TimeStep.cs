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
using Sandwych.Hmm;

namespace Sandwych.MapMatchingKit
{
    /// <summary>
    /// Contains everything the hmm-lib needs to process a new time step including emisson and 
    /// observation probabilities.
    /// </summary>
    /// <typeparam name="TState">road position type, which corresponds to the HMM state.</typeparam>
    /// <typeparam name="TObservation">location measurement type, which corresponds to the HMM observation.</typeparam>
    /// <typeparam name="TRoadPath">road path object</typeparam>
    public class TimeStep<TState, TObservation, TRoadPath>
    {

        /// <summary>
        /// Observation made at this time step.
        /// </summary>
        public TObservation Observation { get; }

        /// <summary>
        /// State candidates at this time step.
        /// </summary>
        public IEnumerable<TState> Candidates { get; }


        /// <summary>
        /// Road paths between all candidates pairs of the previous and the current time step.
        /// </summary>
        public Dictionary<Transition<TState>, TRoadPath> RoadPaths { get; }

        public Dictionary<TState, double> EmissionLogProbabilities { get; }

        public Dictionary<Transition<TState>, double> TransitionLogProbabilities { get; }


        public TimeStep(in TObservation observation, IEnumerable<TState> candidates)
        {
            this.EmissionLogProbabilities = new Dictionary<TState, double>();
            this.TransitionLogProbabilities = new Dictionary<Transition<TState>, double>();
            this.RoadPaths = new Dictionary<Transition<TState>, TRoadPath>();
            this.Observation = observation;
            this.Candidates = candidates;
        }

        public void AddEmissionLogProbability(TState candidate, double emissionLogProbability)
        {
            if (this.EmissionLogProbabilities.ContainsKey(candidate))
            {
                throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate has already been added.");
            }
            this.EmissionLogProbabilities.Add(candidate, emissionLogProbability);
        }

        /// <summary>
        /// Does not need to be called for non-existent transitions.
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="toPosition"></param>
        /// <param name="transitionLogProbability"></param>
        public void AddTransitionLogProbability(TState fromPosition, TState toPosition,
                                                double transitionLogProbability)
        {
            var transition = new Transition<TState>(fromPosition, toPosition);
            if (this.TransitionLogProbabilities.ContainsKey(transition))
            {
                throw new ArgumentOutOfRangeException(nameof(transition), "Transition has already been added.");
            }
            this.TransitionLogProbabilities.Add(transition, transitionLogProbability);
        }

        /// <summary>
        /// Does not need to be called for non-existent transitions.
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="toPosition"></param>
        /// <param name="roadPath"></param>
        public void AddRoadPath(TState fromPosition, TState toPosition, TRoadPath roadPath)
        {
            var transition = new Transition<TState>(fromPosition, toPosition);
            if (this.RoadPaths.ContainsKey(transition))
            {
                throw new ArgumentOutOfRangeException(nameof(transition), "Transition has already been added.");
            }
            this.RoadPaths.Add(transition, roadPath);
        }

    }
}
