using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Hmm
{
    /// <summary>
    /// Stores addition information for each candidate.
    /// </summary>
    internal class Candidate<TState, TObservation, TDescriptor>
    {

        private readonly TState _state;

        /// <summary>
        /// * Back pointer to previous state candidate in the most likely sequence.
        /// * Back pointers are chained using plain references.
        /// * This allows garbage collection of unreachable back pointers.
        /// </summary>
        private readonly Candidate<TState, TObservation, TDescriptor> _backPointer;

        private readonly TObservation _observation;

        private readonly TDescriptor _transitionDescriptor;

        public TState State => _state;

        public TObservation Observation => _observation;

        public TDescriptor TransitionDescriptor => _transitionDescriptor;

        public Candidate<TState, TObservation, TDescriptor> BackPointer => _backPointer;

        public Candidate(in TState state, in Candidate<TState, TObservation, TDescriptor> backPointer, in TObservation observation, in TDescriptor transitionDescriptor)
        {
            this._state = state;
            this._backPointer = backPointer;
            this._observation = observation;
            this._transitionDescriptor = transitionDescriptor;
        }
    }
}
