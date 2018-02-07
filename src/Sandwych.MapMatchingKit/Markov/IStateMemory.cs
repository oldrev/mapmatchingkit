using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    /// <summary>
    /// State memory in Hidden Markov Model (HMM) inference and organizes state vectors
    /// <i>S<sub>t</sub></i>, i.e. a set of state candidates representing possible states for some time
    /// <i>t</i> with a probability distribution, over time.
    /// </summary>
    /// <typeparam name="TCandidate">Candidate inherits from {@link StateCandidate}.</typeparam>
    /// <typeparam name="TTransition">Transition inherits from {@link StateTransition}.</typeparam>
    /// <typeparam name="TSample">Sample inherits from {@link Sample}.</typeparam>
    public interface IStateMemory<TCandidate, TTransition, TSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
        where TSample : ISample

    {
        /// <summary>
        /// Indicates if the state is empty.
        /// Boolean indicating if the state is empty.
        /// </summary>
        bool IsEmpty { get; }


        /// <summary>
        /// Gets the size of the state, which is the number of state candidates organized in the data structure.
        /// Size of the state, which is the number of state candidates.
        /// </summary>
        int Count { get; }


        /// <summary>
        /// Sample object of the most recent update.
        /// Sample object of the most recent update or null if there hasn't been any update yet.
        /// </summary>
        TSample Sample { get; }


        /// <summary>
        /// Updates the state with a state vector which is a set of {@link StateCandidate} objects with
        /// its respective measurement, which is a sample object.
        /// </summary>
        /// <param name="vector">vector State vector for update of the state.</param>
        /// <param name="sample">sample Sample measurement of the state vector.</param>
        void Update(ICollection<TCandidate> vector, in TSample sample);


        /// <summary>
        /// Gets state vector of the last update.
        /// </summary>
        /// <returns>State vector of the last update, or an empty set if there hasn't been any update yet.</returns>
        ICollection<TCandidate> Vector();

        /// <summary>
        /// Gets a state estimate which is the most likely state candidate of the last update, with
        /// respect to state candidate's filter probability (<see cref="AbstractStateCandidate{TCandidate, TTransition, TSample}.Filtprob"/>).
        /// </summary>
        /// <returns>State estimate, which is most likely state candidate.</returns>
        TCandidate Estimate();

    }

}
