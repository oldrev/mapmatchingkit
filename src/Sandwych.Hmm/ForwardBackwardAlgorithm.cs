/**
 * Copyright (C) 2016, BMW AG
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
using System.Linq;
using System.Diagnostics;

namespace Sandwych.Hmm
{
    /// <summary>
    /// Computes the forward-backward algorithm, also known as smoothing.
    /// This algorithm computes the probability of each state candidate at each time step given the
    /// entire observation sequence.
    /// </summary>
    /// <typeparam name="TState">The state type</typeparam>
    /// <typeparam name="TObservation">The observation type</typeparam>
    public sealed class ForwardBackwardAlgorithm<TState, TObservation>
    {

        /// <summary>
        /// Internal state of each time step.
        /// </summary>
        private struct Step
        {
            public readonly IEnumerable<TState> _candidates;
            public readonly IReadOnlyDictionary<TState, double> _emissionProbabilities;
            public readonly IReadOnlyDictionary<Transition<TState>, double> _transitionProbabilities;
            public readonly IReadOnlyDictionary<TState, double> _forwardProbabilities;
            public readonly double _scalingDivisor; // Normalizes sum of forward probabilities to 1.

            public Step(IEnumerable<TState> candidates, IReadOnlyDictionary<TState, double> emissionProbabilities,
                    IReadOnlyDictionary<Transition<TState>, double> transitionProbabilities,
                    IReadOnlyDictionary<TState, double> forwardProbabilities, double scalingDivisor)
            {
                this._candidates = candidates;
                this._emissionProbabilities = emissionProbabilities;
                this._transitionProbabilities = transitionProbabilities;
                this._forwardProbabilities = forwardProbabilities;
                this._scalingDivisor = scalingDivisor;
            }
        }

        private const double Delta = 1e-8;

        private IList<Step> _steps;
        private IEnumerable<TState> _prevCandidates; // For on-the-fly computation of forward probabilities

        /*
         * @throws NullPointerException if any initial probability is missing
         * @throws IllegalStateException if this method or
         * {@link #startWithInitialObservation(Object, Collection, Map)} has already been called
         */
        /// <summary>
        /// Lets the computation start with the given initial state probabilities.
        /// </summary>
        /// <param name="initialStates">initialStates Pass a collection with predictable iteration order 
        /// such as {@link ArrayList} to ensure deterministic results. 
        /// </param>
        /// <param name="initialProbabilities">initialProbabilities Initial probabilities for each initial state. </param>
        public void StartWithInitialStateProbabilities(IEnumerable<TState> initialStates,
            IReadOnlyDictionary<TState, double> initialProbabilities)
        {
            if (!SumsToOne(initialProbabilities.Values))
            {
                throw new ArgumentException(nameof(initialStates), "Initial state probabilities must sum to 1.");
            }

            InitializeStateProbabilities(default(TObservation), initialStates, initialProbabilities);
        }

        /**
         * Lets the computation start at the given first observation.
         *
         * @param candidates Pass a collection with predictable iteration order such as
         * {@link ArrayList} to ensure deterministic results.
         *
         * @param emissionProbabilities Emission probabilities of the first observation for
         * each of the road position candidates.
         *
         * @throws NullPointerException if any emission probability is missing
         *
         * @throws IllegalStateException if this method or
         * {@link #startWithInitialStateProbabilities(Collection, Map)}} has already been called
         */
        public void StartWithInitialObservation(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> emissionProbabilities)
        {
            InitializeStateProbabilities(observation, candidates, emissionProbabilities);
        }

        /**
         * Processes the next time step.
         *
         * @param candidates Pass a collection with predictable iteration order such as
         * {@link ArrayList} to ensure deterministic results.
         *
         * @param emissionProbabilities Emission probabilities for each candidate state.
         *
         * @param transitionProbabilities Transition probability between all pairs of candidates.
         * A transition probability of zero is assumed for every missing transition.
         *
         * @throws NullPointerException if any emission probability is missing
         *
         * @throws IllegalStateException if neither
         * {@link #startWithInitialStateProbabilities(Collection, Map)} nor
         * {@link #startWithInitialObservation(Object, Collection, Map)} has not been called before
         */
        public void NextStep(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> emissionProbabilities,
                IReadOnlyDictionary<Transition<TState>, double> transitionProbabilities)
        {
            if (_steps == null)
            {
                throw new InvalidOperationException("startWithInitialStateProbabilities(...) or " +
                        "startWithInitialObservation(...) must be called first.");
            }

            // Make defensive copies.
            // candidates = new List<TState>(candidates);
            emissionProbabilities = new Dictionary<TState, double>(emissionProbabilities.ToDictionary(p => p.Key, p => p.Value));
            transitionProbabilities = new Dictionary<Transition<TState>, double>(transitionProbabilities.ToDictionary(p => p.Key, p => p.Value));

            // On-the-fly computation of forward probabilities at each step allows to efficiently
            // (re)compute smoothing probabilities at any time step.
            var prevForwardProbabilities = _steps[_steps.Count - 1]._forwardProbabilities;
            var curForwardProbabilities = new Dictionary<TState, double>(candidates.Count());
            var sum = default(double);
            foreach (var curState in candidates)
            {
                var forwardProbability = ComputeForwardProbability(curState,
                        prevForwardProbabilities, emissionProbabilities, transitionProbabilities);
                curForwardProbabilities[curState] = forwardProbability;
                sum += forwardProbability;
            }

            NormalizeForwardProbabilities(curForwardProbabilities, sum);
            _steps.Add(new Step(candidates, emissionProbabilities, transitionProbabilities,
                    curForwardProbabilities, sum));

            _prevCandidates = candidates;
        }

        /// <summary>
        /// Returns the probability for all candidates of all time steps given all observations. <br/>
        /// The time steps include the initial states/observations time step.
        /// </summary>
        /// <returns></returns>
        public List<IReadOnlyDictionary<TState, double>> ComputeSmoothingProbabilities()
        {
            return ComputeSmoothingProbabilities(null);
        }

        /// <summary>
        /// Returns the probability of the specified candidate at the specified zero-based time step
        /// given the observations up to t.
        /// </summary>
        public double GetForwardProbability(int t, TState candidate)
        {
            if (_steps == null)
            {
                throw new InvalidOperationException("No time steps yet.");
            }

            return _steps[t]._forwardProbabilities[candidate];
        }


        /// <summary>
        /// Returns the probability of the specified candidate given all previous observations.
        /// </summary>
        public double GetCurrentForwardProbability(TState candidate)
        {
            if (_steps == null)
            {
                throw new InvalidOperationException("No time steps yet.");
            }

            return GetForwardProbability(_steps.Count - 1, candidate);
        }

        /// <summary>
        /// Returns the log probability of the entire observation sequence.
        /// The log is returned to prevent arithmetic underflows for very small probabilities.
        /// </summary>
        public double GetObservationLogProbability()
        {
            if (_steps == null)
            {
                throw new InvalidOperationException("No time steps yet.");
            }

            var result = default(double);
            foreach (var step in _steps)
            {
                result += Math.Log(step._scalingDivisor);
            }
            return result;
        }

        /// <summary>
        /// <see cref="ComputeSmoothingProbabilities"/>
        /// </summary>
        /// <param name="outBackwardProbabilities">outBackwardProbabilities optional output parameter for backward probabilities, 
        /// must be empty if not null.
        /// </param>
        /// <returns></returns>
        List<IReadOnlyDictionary<TState, double>> ComputeSmoothingProbabilities(
                List<IReadOnlyDictionary<TState, double>> outBackwardProbabilities)
        {
            //assert outBackwardProbabilities == null || outBackwardProbabilities.isEmpty();
            Debug.Assert(outBackwardProbabilities == null || outBackwardProbabilities.Count == 0);

            var result = new List<IReadOnlyDictionary<TState, double>>();

            var stepIter = _steps.AsEnumerable().Reverse().GetEnumerator();
            if (!stepIter.MoveNext())
            {
                return result;
            }

            // Initial step
            var step = stepIter.Current;
            var backwardProbabilities = new Dictionary<TState, double>(step._candidates.Count());
            foreach (TState candidate in step._candidates)
            {
                backwardProbabilities[candidate] = 1.0f;
            }
            if (outBackwardProbabilities != null)
            {
                outBackwardProbabilities.Add(backwardProbabilities);
            }
            result.Add(ComputeSmoothingProbabilitiesVector(step._candidates, step._forwardProbabilities, backwardProbabilities));

            // Remaining steps
            while (stepIter.MoveNext())
            {
                var nextStep = step;
                step = stepIter.Current;
                var nextBackwardProbabilities = backwardProbabilities;
                backwardProbabilities = new Dictionary<TState, double>(step._candidates.Count());
                foreach (TState candidate in step._candidates)
                {
                    // Using the scaling divisors of the next steps eliminates the need to
                    // normalize the smoothing probabilities,
                    // see also https://en.wikipedia.org/wiki/Forward%E2%80%93backward_algorithm.
                    double probability = ComputeUnscaledBackwardProbability(candidate,
                            nextBackwardProbabilities, nextStep) / nextStep._scalingDivisor;
                    backwardProbabilities[candidate] = probability;
                }
                if (outBackwardProbabilities != null)
                {
                    outBackwardProbabilities.Add(backwardProbabilities);
                }
                result.Add(ComputeSmoothingProbabilitiesVector(step._candidates,
                        step._forwardProbabilities, backwardProbabilities));
            }
            result.Reverse();
            return result;
        }

        private IReadOnlyDictionary<TState, double> ComputeSmoothingProbabilitiesVector(IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> forwardProbabilities, IReadOnlyDictionary<TState, double> backwardProbabilities)
        {
            //assert forwardProbabilities.size() == backwardProbabilities.size();
            Debug.Assert(forwardProbabilities.Count == backwardProbabilities.Count);
            var result = new Dictionary<TState, double>(candidates.Count());
            foreach (TState state in candidates)
            {
                var probability = forwardProbabilities[state] * backwardProbabilities[state];
                //assert Utils.probabilityInRange(probability, DELTA);
                Debug.Assert(HmmUtils.ProbabilityInRange(probability, Delta));
                result[state] = probability;
            }
            //assert sumsToOne(result.values());
            Debug.Assert(SumsToOne(result.Values));
            return result;
        }

        private double ComputeUnscaledBackwardProbability(TState candidate,
                IReadOnlyDictionary<TState, double> nextBackwardProbabilities, Step nextStep)
        {
            var result = default(double);
            foreach (TState nextCandidate in nextStep._candidates)
            {
                result += nextStep._emissionProbabilities[nextCandidate] *
                        nextBackwardProbabilities[nextCandidate] * GetTransitionProbability(
                        candidate, nextCandidate, nextStep._transitionProbabilities);
            }
            return result;
        }

        private bool SumsToOne(IEnumerable<double> probabilities)
        {
            var sum = default(double);
            foreach (var probability in probabilities)
            {
                sum += probability;
            }
            return Math.Abs(sum - 1.0) <= Delta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="observation">observation Use only if HMM only starts with first observation.</param>
        /// <param name="candidates"></param>
        /// <param name="initialProbabilities"></param>
        private void InitializeStateProbabilities(
            TObservation observation, IEnumerable<TState> candidates, IReadOnlyDictionary<TState, double> initialProbabilities)
        {
            if (_steps != null)
            {
                throw new InvalidOperationException("Initial probabilities have already been set.");
            }

            //candidates = new List<TState>(candidates); // Defensive copy
            _steps = new List<Step>();

            var forwardProbabilities = new Dictionary<TState, double>(candidates.Count());
            var sum = 0.0;
            foreach (TState candidate in candidates)
            {
                var forwardProbability = initialProbabilities[candidate];
                forwardProbabilities[candidate] = forwardProbability;
                sum += forwardProbability;
            }

            NormalizeForwardProbabilities(forwardProbabilities, sum);
            _steps.Add(new Step(candidates, null, null, forwardProbabilities, sum));

            _prevCandidates = candidates;
        }

        /// <summary>
        /// Returns the non-normalized forward probability of the specified state.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="prevForwardProbabilities"></param>
        /// <param name="emissionProbabilities"></param>
        /// <param name="transitionProbabilities"></param>
        /// <returns></returns>
        private double ComputeForwardProbability(TState currentState,
                IReadOnlyDictionary<TState, double> prevForwardProbabilities, IReadOnlyDictionary<TState, double> emissionProbabilities,
                IReadOnlyDictionary<Transition<TState>, double> transitionProbabilities)
        {
            var result = default(double);
            foreach (TState prevState in _prevCandidates)
            {
                result += prevForwardProbabilities[prevState] *
                        GetTransitionProbability(prevState, currentState, transitionProbabilities);
            }
            result *= emissionProbabilities[currentState];
            return result;
        }

        /// <summary>
        /// Returns zero probability for non-existing transitions.
        /// </summary>
        /// <param name="prevState"></param>
        /// <param name="curState"></param>
        /// <param name="transitionProbabilities"></param>
        /// <returns></returns>
        private static double GetTransitionProbability(TState prevState, TState curState,
                IReadOnlyDictionary<Transition<TState>, double> transitionProbabilities)
        {
            return transitionProbabilities.TryGetValue(new Transition<TState>(prevState, curState), out var transitionProbability) ? transitionProbability : 0.0;
        }

        private void NormalizeForwardProbabilities(
                IDictionary<TState, double> forwardProbabilities, double sum)
        {
            foreach (var key in forwardProbabilities.Keys.ToArray())
            {
                forwardProbabilities[key] = forwardProbabilities[key] / sum;
            }
        }

    }

}
