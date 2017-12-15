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

        public TState State { get; }

        public TObservation Observation { get; }

        public TDescriptor TransitionDescriptor { get; }

        /// <summary>
        /// * Back pointer to previous state candidate in the most likely sequence.
        /// * Back pointers are chained using plain references.
        /// * This allows garbage collection of unreachable back pointers.
        /// </summary>
        public Candidate<TState, TObservation, TDescriptor> BackPointer { get; }

        public Candidate(in TState state, in Candidate<TState, TObservation, TDescriptor> backPointer, in TObservation observation, in TDescriptor transitionDescriptor)
        {
            this.State = state;
            this.BackPointer = backPointer;
            this.Observation = observation;
            this.TransitionDescriptor = transitionDescriptor;
        }
    }
}
