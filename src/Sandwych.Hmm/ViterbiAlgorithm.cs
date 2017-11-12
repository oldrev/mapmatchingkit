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
using System.Diagnostics;
using System.Linq;

namespace Sandwych.Hmm
{
    /**
     *
     * @param <S> the state type
     * @param <O> the observation type
     * @param <D> the transition descriptor type. Pass {@link Object} if transition descriptors are not
     * needed.
     */
    /// <summary>
    /// Implementation of the Viterbi algorithm for time-inhomogeneous Markov processes,
    /// meaning that the set of states and state transition probabilities are not necessarily fixed
    /// for all time steps. The plain Viterbi algorithm for stationary Markov processes is described e.g.
    /// in Rabiner, Juang, An introduction to Hidden Markov Models, IEEE ASSP Mag., pp 4-16, June 1986.
    ///
    /// <p>Generally expects logarithmic probabilities as input to prevent arithmetic underflows for
    /// small probability values.
    /// 
    /// <p>This algorithm supports storing transition objects in
    /// {@link #nextStep(Object, Collection, Map, Map, Map)}. For instance if a HMM is
    /// used for map matching, this could be routes between road position candidates.
    /// The transition descriptors of the most likely sequence can be retrieved later in
    /// {@link SequenceState#transitionDescriptor} and hence do not need to be stored by the
    /// caller. Since the caller does not know in advance which transitions will occur in the most
    /// likely sequence, this reduces the number of transitions that need to be kept in memory
    /// from t*n² to t*n since only one transition descriptor is stored per back pointer,
    /// where t is the number of time steps and n the number of candidates per time step.
    ///
    /// <p>For long observation sequences, back pointers usually converge to a single path after a
    /// certain number of time steps. For instance, when matching GPS coordinates to roads, the last
    /// GPS positions in the trace usually do not affect the first road matches anymore.
    /// This implementation exploits this fact by letting the Java garbage collector
    /// take care of unreachable back pointers. If back pointers converge to a single path after a
    /// constant number of time steps, only O(t) back pointers and transition descriptors need to be
    /// stored in memory.
    /// </summary>
    public sealed class ViterbiAlgorithm<TState, TObservation, TDescriptor>
    {

        /// <summary>
        /// Stores addition information for each candidate.
        /// </summary>
        private class ExtendedState
        {

            public readonly TState _state;

            /// <summary>
            /// * Back pointer to previous state candidate in the most likely sequence.
            /// * Back pointers are chained using plain references.
            /// * This allows garbage collection of unreachable back pointers.
            /// </summary>
            public readonly ExtendedState _backPointer;

            public readonly TObservation _observation;

            public readonly TDescriptor _transitionDescriptor;

            public ExtendedState(TState state, ExtendedState backPointer, TObservation observation, TDescriptor transitionDescriptor)
            {
                this._state = state;
                this._backPointer = backPointer;
                this._observation = observation;
                this._transitionDescriptor = transitionDescriptor;
            }
        }

        private struct ForwardStepResult
        {
            public readonly IDictionary<TState, double> _newMessage;

            /**
             * Includes back pointers to previous state candidates for retrieving the most likely
             * sequence after the forward pass.
             */
            public readonly IDictionary<TState, ExtendedState> _newExtendedStates;

            public ForwardStepResult(int numberStates)
            {
                _newMessage = new Dictionary<TState, double>(HmmUtils.InitialHashMapCapacity(numberStates));
                _newExtendedStates = new Dictionary<TState, ExtendedState>(HmmUtils.InitialHashMapCapacity(numberStates));
            }
        }

        /// <summary>
        /// Allows to retrieve the most likely sequence using back pointers.
        /// </summary>
        private IDictionary<TState, ExtendedState> _lastExtendedStates;

        private IEnumerable<TState> _prevCandidates;

        /**
         * For each state s_t of the current time step t, message.get(s_t) contains the log
         * probability of the most likely sequence ending in state s_t with given observations
         * o_1, ..., o_t.
         *
         * Formally, this is max log p(s_1, ..., s_t, o_1, ..., o_t) w.r.t. s_1, ..., s_{t-1}.
         * Note that to compute the most likely state sequence, it is sufficient and more
         * efficient to compute in each time step the joint probability of states and observations
         * instead of computing the conditional probability of states given the observations.
         */
        private IDictionary<TState, double> _message;

        private bool _isBroken = false;

        private ForwardBackwardAlgorithm<TState, TObservation> _forwardBackward;

        private IList<IDictionary<TState, double>> _messageHistory; // For debugging only.

        /// <summary>
        /// Need to construct a new instance for each sequence of observations.
        /// </summary>
        public ViterbiAlgorithm()
        {
        }

        /**
         * Whether to store intermediate forward messages
         * (probabilities of intermediate most likely paths) for debugging.
         * Default: false
         * Must be called before processing is started.
         */
        public ViterbiAlgorithm<TState, TObservation, TDescriptor> SetKeepMessageHistory(bool keepMessageHistory)
        {
            if (processingStarted())
            {
                throw new InvalidOperationException("Processing has already started.");
            }

            if (keepMessageHistory)
            {
                _messageHistory = new List<IDictionary<TState, double>>();
            }
            else
            {
                _messageHistory = null;
            }
            return this;
        }

        /**
         * Whether to compute smoothing probabilities using the {@link ForwardBackwardAlgorithm}
         * for the states of the most likely sequence. Note that this significantly increases
         * computation time and memory footprint.
         * Default: false
         * Must be called before processing is started.
         */
        public ViterbiAlgorithm<TState, TObservation, TDescriptor> SetComputeSmoothingProbabilities(bool computeSmoothingProbabilities)
        {
            if (processingStarted())
            {
                throw new InvalidOperationException("Processing has already started.");
            }

            if (computeSmoothingProbabilities)
            {
                _forwardBackward = new ForwardBackwardAlgorithm<TState, TObservation>();
            }
            else
            {
                _forwardBackward = null;
            }
            return this;
        }

        /**
         * Returns whether {@link #startWithInitialObservation(Object, Collection, Map)}
         * or {@link #startWithInitialStateProbabilities(Collection, Map)} has already been called.
         */
        public bool processingStarted()
        {
            return _message != null;
        }

        /**
         * Lets the HMM computation start with the given initial state probabilities.
         *
         * @param initialStates Pass a collection with predictable iteration order such as
         * {@link ArrayList} to ensure deterministic results.
         * @param initialLogProbabilities Initial log probabilities for each initial state.
         *
         * @throws NullPointerException if any initial probability is missing
         *
         * @throws IllegalStateException if this method or
         * {@link #startWithInitialObservation(Object, Collection, Map)}
         * has already been called
         */
        public void startWithInitialStateProbabilities(IEnumerable<TState> initialStates, IReadOnlyDictionary<TState, double> initialLogProbabilities)
        {
            InitializeStateProbabilities(default(TObservation), initialStates, initialLogProbabilities);

            if (_forwardBackward != null)
            {
                _forwardBackward.StartWithInitialStateProbabilities(initialStates,
                        HmmUtils.LogToNonLogProbabilities(initialLogProbabilities));
            }
        }

        /**
         * Lets the HMM computation start at the given first observation and uses the given emission
         * probabilities as the initial state probability for each starting state s.
         *
         * @param candidates Pass a collection with predictable iteration order such as
         * {@link ArrayList} to ensure deterministic results.
         * @param emissionLogProbabilities Emission log probabilities of the first observation for
         * each of the road position candidates.
         *
         * @throws NullPointerException if any emission probability is missing
         *
         * @throws IllegalStateException if this method or
         * {@link #startWithInitialStateProbabilities(Collection, Map)}} has already been called
         */
        public void StartWithInitialObservation(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> emissionLogProbabilities)
        {
            InitializeStateProbabilities(observation, candidates, emissionLogProbabilities);

            if (_forwardBackward != null)
            {
                _forwardBackward.StartWithInitialObservation(observation, candidates,
                        HmmUtils.LogToNonLogProbabilities(emissionLogProbabilities));
            }
        }

        /**
         * Processes the next time step. Must not be called if the HMM is broken.
         *
         * @param candidates Pass a collection with predictable iteration order such as
         * {@link ArrayList} to ensure deterministic results.
         * @param emissionLogProbabilities Emission log probabilities for each candidate state.
         *
         * @param transitionLogProbabilities Transition log probability between all pairs of candidates.
         * A transition probability of zero is assumed for every missing transition.
         *
         * @param transitionDescriptors Optional objects that describes the transitions.
         *
         * @throws NullPointerException if any emission probability is missing
         *
         * @throws IllegalStateException if neither
         * {@link #startWithInitialStateProbabilities(Collection, Map)} nor
         * {@link #startWithInitialObservation(Object, Collection, Map)}
         * has not been called before or if this method is called after an HMM break has occurred
         */
        public void NextStep(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> emissionLogProbabilities,
                IReadOnlyDictionary<Transition<TState>, double> transitionLogProbabilities,
                IReadOnlyDictionary<Transition<TState>, TDescriptor> transitionDescriptors)
        {
            if (!processingStarted())
            {
                throw new InvalidOperationException(
                        "startWithInitialStateProbabilities() or startWithInitialObservation() "
                        + "must be called first.");
            }
            if (_isBroken)
            {
                throw new InvalidOperationException("Method must not be called after an HMM break.");
            }

            // Forward step
            var forwardStepResult = ForwardStep(observation, _prevCandidates,
                    candidates, _message, emissionLogProbabilities, transitionLogProbabilities,
                    transitionDescriptors);
            _isBroken = HmmBreak(forwardStepResult._newMessage);
            if (_isBroken)
            {
                return;
            }
            if (_messageHistory != null)
            {
                _messageHistory.Add(forwardStepResult._newMessage);
            }
            _message = forwardStepResult._newMessage;
            _lastExtendedStates = forwardStepResult._newExtendedStates;

            _prevCandidates = new List<TState>(candidates); // Defensive copy.

            if (_forwardBackward != null)
            {
                _forwardBackward.NextStep(observation, candidates,
                        HmmUtils.LogToNonLogProbabilities(emissionLogProbabilities),
                        HmmUtils.LogToNonLogProbabilities(transitionLogProbabilities));
            }
        }

        /**
         * See {@link #nextStep(Object, Collection, Map, Map, Map)}
         */
        public void NextStep(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> emissionLogProbabilities,
                IReadOnlyDictionary<Transition<TState>, double> transitionLogProbabilities)
        {
            NextStep(observation, candidates, emissionLogProbabilities, transitionLogProbabilities,
                    new Dictionary<Transition<TState>, TDescriptor>());
        }

        /**
         * Returns the most likely sequence of states for all time steps. This includes the initial
         * states / initial observation time step. If an HMM break occurred in the last time step t,
         * then the most likely sequence up to t-1 is returned. See also {@link #isBroken()}.
         *
         * <p>Formally, the most likely sequence is argmax p([s_0,] s_1, ..., s_T | o_1, ..., o_T)
         * with respect to s_1, ..., s_T, where s_t is a state candidate at time step t,
         * o_t is the observation at time step t and T is the number of time steps.
         */
        public IReadOnlyList<SequenceState<TState, TObservation, TDescriptor>> computeMostLikelySequence()
        {
            if (_message == null)
            {
                // Return empty most likely sequence if there are no time steps or if initial
                // observations caused an HMM break.
                return new List<SequenceState<TState, TObservation, TDescriptor>>();
            }
            else
            {
                return RetrieveMostLikelySequence();
            }
        }

        /**
         * Returns whether an HMM occurred in the last time step.
         *
         * An HMM break means that the probability of all states equals zero.
         */
        public bool IsBroken()
        {
            return _isBroken;
        }

        /**
         * @see #setComputeSmoothingProbabilities(boolean)
         */
        public bool IsComputeSmoothingProbabilities => _forwardBackward != null;

        /**
         * @see #setKeepMessageHistory(boolean)
         */
        public bool IsKeepMessageHistory => _messageHistory != null;

        /**
         *  Returns the sequence of intermediate forward messages for each time step.
         *  Returns null if message history is not kept.
         */
        public IList<IDictionary<TState, double>> MessageHistory => _messageHistory;

        public String GetMessageHistoryString()
        {
            if (_messageHistory == null)
            {
                throw new InvalidOperationException("Message history was not recorded.");
            }

            var sb = new StringBuilder();
            sb.Append("Message history with log probabilies\n\n");
            int i = 0;
            foreach (var message in _messageHistory)
            {
                sb.Append("Time step " + i + "\n");
                i++;
                foreach (TState state in message.Keys)
                {
                    sb.Append(state + ": " + message[state] + "\n");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        /**
         * Returns whether the specified message is either empty or only contains state candidates
         * with zero probability and thus causes the HMM to break.
         */
        private bool HmmBreak(IDictionary<TState, double> message)
        {
            foreach (var logProbability in message.Values)
            {
                if (logProbability != double.NegativeInfinity)
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * @param observation Use only if HMM only starts with first observation.
         */
        private void InitializeStateProbabilities(TObservation observation, IEnumerable<TState> candidates,
                IReadOnlyDictionary<TState, double> initialLogProbabilities)
        {
            if (processingStarted())
            {
                throw new InvalidOperationException("Initial probabilities have already been set.");
            }

            // Set initial log probability for each start state candidate based on first observation.
            // Do not assign initialLogProbabilities directly to message to not rely on its iteration
            // order.
            var initialMessage = new Dictionary<TState, double>(candidates.Count());
            foreach (var candidate in candidates)
            {
                if (!initialLogProbabilities.TryGetValue(candidate, out var logProbability))
                {
                    throw new NullReferenceException("No initial probability for " + candidate);
                }
                initialMessage[candidate] = logProbability;
            }

            _isBroken = HmmBreak(initialMessage);
            if (_isBroken)
            {
                return;
            }

            _message = initialMessage;
            if (_messageHistory != null)
            {
                _messageHistory.Add(_message);
            }

            _lastExtendedStates = new Dictionary<TState, ExtendedState>(candidates.Count());
            foreach (TState candidate in candidates)
            {
                _lastExtendedStates[candidate] = new ExtendedState(candidate, null, observation, default(TDescriptor));
            }

            _prevCandidates = new List<TState>(candidates); // Defensive copy.
        }

        /**
         * Computes the new forward message and the back pointers to the previous states.
         *
         * @throws NullPointerException if any emission probability is missing
         */
        private ForwardStepResult ForwardStep(TObservation observation, IEnumerable<TState> prevCandidates,
                IEnumerable<TState> curCandidates, IDictionary<TState, double> message,
                IReadOnlyDictionary<TState, double> emissionLogProbabilities,
                IReadOnlyDictionary<Transition<TState>, double> transitionLogProbabilities,
                IReadOnlyDictionary<Transition<TState>, TDescriptor> transitionDescriptors)
        {
            var result = new ForwardStepResult(curCandidates.Count());
            Debug.Assert(prevCandidates.Count() > 0);

            foreach (var curState in curCandidates)
            {
                var maxLogProbability = Double.NegativeInfinity;
                var hasMaxPrevState = false;
                var maxPrevState = default(TState);
                foreach (var prevState in prevCandidates)
                {
                    var logProbability = message[prevState] + TransitionLogProbability(
                            prevState, curState, transitionLogProbabilities);
                    if (logProbability > maxLogProbability)
                    {
                        maxLogProbability = logProbability;
                        maxPrevState = prevState;
                        hasMaxPrevState = true;
                    }
                }
                // Throws NullPointerException if curState is not stored in the map.
                result._newMessage[curState] = maxLogProbability + emissionLogProbabilities[curState];

                // Note that maxPrevState == null if there is no transition with non-zero probability.
                // In this case curState has zero probability and will not be part of the most likely
                // sequence, so we don't need an ExtendedState.
                if (hasMaxPrevState)
                {
                    var transition = new Transition<TState>(maxPrevState, curState);
                    var extendedState = new ExtendedState(curState,
                        _lastExtendedStates.TryGetValue(maxPrevState, out var bp) ? bp : default(ExtendedState),
                        observation,
                        transitionDescriptors.TryGetValue(transition, out var td) ? td : default(TDescriptor));
                    result._newExtendedStates[curState] = extendedState;
                }
            }
            return result;
        }

        private double TransitionLogProbability(TState prevState, TState curState,
            IReadOnlyDictionary<Transition<TState>, double> transitionLogProbabilities)
        {
            if (!transitionLogProbabilities.TryGetValue(new Transition<TState>(prevState, curState), out var transitionLogProbability))
            {

                return double.NegativeInfinity; // Transition has zero probability.
            }
            else
            {

                return transitionLogProbability;
            }
        }

        /**
         * Retrieves the first state of the current forward message with maximum probability.
         */
        private TState GetMostLikelyState()
        {
            // Otherwise an HMM break would have occurred and message would be null.
            Debug.Assert(_message.Count > 0);

            TState result = default(TState);
            var maxLogProbability = double.NegativeInfinity;
            foreach (var entry in _message)
            {
                if (entry.Value > maxLogProbability)
                {
                    result = entry.Key;
                    maxLogProbability = entry.Value;
                }
            }

            //Debug.Assert(result != null); // Otherwise an HMM break would have occurred.
            return result;
        }

        /**
         * Retrieves most likely sequence from the internal back pointer sequence.
         */
        private IReadOnlyList<SequenceState<TState, TObservation, TDescriptor>> RetrieveMostLikelySequence()
        {
            // Otherwise an HMM break would have occurred and message would be null.
            Debug.Assert(_message.Count != 0);

            var lastState = GetMostLikelyState();

            // Retrieve most likely state sequence in reverse order
            var result = new List<SequenceState<TState, TObservation, TDescriptor>>();
            var es = _lastExtendedStates.TryGetValue(lastState, out var esv) ? esv : default(ExtendedState);
            IEnumerator<IReadOnlyDictionary<TState, double>> smoothingIter = null;
            if (_forwardBackward != null)
            {
                var smoothingProbabilities =
                        _forwardBackward.ComputeSmoothingProbabilities();
                smoothingIter = smoothingProbabilities.AsEnumerable().Reverse().GetEnumerator();
            }
            else
            {
                smoothingIter = null;
            }
            while (es != null)
            {
                double smoothingProbability = double.NaN;
                if (_forwardBackward != null)
                {
                    // Number of time steps is the same for Viterbi and ForwardBackward algorithm.
                    var hasNext = smoothingIter.MoveNext();
                    Debug.Assert(hasNext);
                    var smoothingProbabilitiesVector = smoothingIter.Current;
                    smoothingProbability = smoothingProbabilitiesVector.TryGetValue(es._state, out var prob) ? prob : double.NaN;
                }
                else
                {
                    smoothingProbability = double.NaN;
                }
                var ss = new SequenceState<TState, TObservation, TDescriptor>(es._state, es._observation, es._transitionDescriptor, smoothingProbability);
                result.Add(ss);
                es = es._backPointer;
            }

            result.Reverse();
            return result;
        }


    }

}
