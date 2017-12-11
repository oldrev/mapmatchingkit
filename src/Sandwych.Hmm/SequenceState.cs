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

namespace Sandwych.Hmm
{

    /// <summary>
    /// State of the most likely sequence with additional information.
    /// </summary>
    /// <typeparam name="TState">The state type</typeparam>
    /// <typeparam name="TObservation">The observation type</typeparam>
    /// <typeparam name="TDescriptor">The transition descriptor type</typeparam>
    public readonly struct SequenceState<TState, TObservation, TDescriptor>
    {
        private readonly TState _state;
        private readonly TObservation _observation;
        private readonly TDescriptor _transitionDescriptor;
        private readonly double _smoothingProbability;

        public TState State => _state;

        /// <summary>
        /// Null if HMM was started with initial state probabilities and state is the initial state.
        /// </summary>
        public TObservation Observation => _observation;


        /// <summary>
        /// Null if transition descriptor was not provided.
        /// </summary>
        public TDescriptor TransitionDescriptor => _transitionDescriptor;

        /// <summary>
        /// Probability of this state given all observations.
        /// </summary>
        public double SmoothingProbability => _smoothingProbability;

        public SequenceState(in TState state, in TObservation observation, in TDescriptor transitionDescriptor, double smoothingProbability)
        {
            _state = state;
            _observation = observation;
            _transitionDescriptor = transitionDescriptor;
            _smoothingProbability = smoothingProbability;
        }

        bool HasSmoothingProbability => this.SmoothingProbability != double.NaN;
    }

}
