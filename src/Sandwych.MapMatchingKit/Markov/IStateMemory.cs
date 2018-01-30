using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    /**
     * State memory in Hidden Markov Model (HMM) inference and organizes state vectors
     * <i>S<sub>t</sub></i>, i.e. a set of state candidates representing possible states for some time
     * <i>t</i> with a probability distribution, over time.
     *
     * @param <C> Candidate inherits from {@link StateCandidate}.
     * @param <T> Transition inherits from {@link StateTransition}.
     * @param <S> Sample inherits from {@link Sample}.
     */
    public interface IStateMemory<TCandidate, TTransition, TSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
        where TSample : ISample

    {
        /**
         * Indicates if the state is empty.
         *
         * @return Boolean indicating if the state is empty.
         */
        bool IsEmpty { get; }

        /**
         * Gets the size of the state, which is the number of state candidates organized in the data
         * structure.
         *
         * @return Size of the state, which is the number of state candidates.
         */
        int Count { get; }

        /**
         * {@link Sample} object of the most recent update.
         *
         * @return {@link Sample} object of the most recent update or null if there hasn't been any
         *         update yet.
         */
        TSample Sample { get; }

        /**
         * Updates the state with a state vector which is a set of {@link StateCandidate} objects with
         * its respective measurement, which is a {@link Sample} object.
         *
         * @param vector State vector for update of the state.
         * @param sample Sample measurement of the state vector.
         */
        void Update(ICollection<TCandidate> vector, in TSample sample);


        /**
         * Gets state vector of the last update.
         *
         * @return State vector of the last update, or an empty set if there hasn't been any update yet.
         */
        ICollection<TCandidate> Vector();

        /**
         * Gets a state estimate which is the most likely state candidate of the last update, with
         * respect to state candidate's filter probability (see {@link StateCandidate#filtprob()}).
         *
         * @return State estimate, which is most likely state candidate.
         */
        TCandidate Estimate();

    }

}
