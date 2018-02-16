using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{

    /// <summary>
    /// <i>k</i>-State data structure for organizing state memory in HMM inference.
    /// </summary>
    /// <typeparam name="TCandidate">Candidate inherits from {@link StateCandidate}.</typeparam>
    /// <typeparam name="TTransition">Transition inherits from {@link StateTransition}.</typeparam>
    /// <typeparam name="TSample">Sample inherits from {@link Sample}.</typeparam>
    public class KState<TCandidate, TTransition, TSample> :
        IStateMemory<TCandidate, TTransition, TSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
        where TSample : ISample
    {
        private readonly int _k;
        private readonly long _t;
        private readonly Deque<(ICollection<TCandidate>, TSample, TCandidate)> _sequence;
        private readonly IDictionary<TCandidate, int> _counters;

        /// <summary>
        /// Creates empty {@link KState} object with default parameters, i.e. capacity is unbounded.
        /// </summary>
        public KState()
        {
            this._k = -1;
            this._t = -1;
            this._sequence = new Deque<(ICollection<TCandidate>, TSample, TCandidate)>();
            this._counters = new Dictionary<TCandidate, int>();
        }


        /*
         * Creates an empty {@link KState} object and sets <i>&kappa;</i> and <i>&tau;</i> parameters.
         *
         * @param k <i>&kappa;</i> parameter bounds the length of the state sequence to at most
         *        <i>&kappa;+1</i> states, if <i>&kappa; &ge; 0</i>.
         * @param t <i>&tau;</i> parameter bounds length of the state sequence to contain only states
         *        for the past <i>&tau;</i> milliseconds.
         */
        public KState(int k, long t)
        {
            this._k = k;
            this._t = t;
            this._sequence = new Deque<(ICollection<TCandidate>, TSample, TCandidate)>();
            this._counters = new Dictionary<TCandidate, int>();
        }

        public bool IsEmpty => _counters.Count == 0;

        public int Count => _counters.Count;

        public TSample Sample
        {
            get
            {
                if (_sequence.Count == 0)
                {
                    return default(TSample);
                }
                else
                {
                    return _sequence.Last().Item2;
                }
            }
        }

        /// <summary>
        /// Gets the sequence of measurements <i>z<sub>0</sub>, z<sub>1</sub>, ..., z<sub>t</sub></i>.
        /// </summary>
        /// <returns>List with the sequence of measurements.</returns>
        public IEnumerable<TSample> Samples()
        {
            foreach (var element in _sequence)
            {
                yield return element.Item2;
            }
        }

        public void Update(ICollection<TCandidate> vector, in TSample sample)
        {
            if (vector.Count == 0)
            {
                return;
            }

            if (_sequence.Count > 0 && _sequence.Last().Item2.Time > sample.Time)
            {
                throw new InvalidOperationException("out-of-order state update is prohibited");
            }

            TCandidate kestimate = null;
            foreach (TCandidate candidate in vector)
            {
                _counters[candidate] = 0;
                if (candidate.Predecessor != null)
                {
                    if (!_counters.ContainsKey(candidate.Predecessor) || !_sequence.Last().Item1.Contains(candidate.Predecessor))
                    {
                        throw new InvalidOperationException("Inconsistent update vector.");
                    }
                    _counters[candidate.Predecessor] = _counters[candidate.Predecessor] + 1;
                }
                if (kestimate == null || candidate.Seqprob > kestimate.Seqprob)
                {
                    kestimate = candidate;
                }
            }

            if (_sequence.Count > 0)
            {
                var last = _sequence.Last();
                var deletes = new HashSet<TCandidate>();

                foreach (TCandidate candidate in last.Item1)
                {
                    if (_counters[candidate] == 0)
                    {
                        deletes.Add(candidate);
                    }
                }

                var size = _sequence.Last().Item1.Count;

                foreach (TCandidate candidate in deletes)
                {
                    if (deletes.Count != size || candidate != last.Item3)
                    {
                        this.Remove(candidate, _sequence.Count - 1);
                    }
                }
            }

            _sequence.AddToBack((vector, sample, kestimate));

            while ((_t > 0 && sample.Time - _sequence.First().Item2.Time > _t)
                    || (_k >= 0 && _sequence.Count > _k + 1))
            {
                var deletes = _sequence.First().Item1;
                _sequence.RemoveFromFront();

                foreach (TCandidate candidate in deletes)
                {
                    _counters.Remove(candidate);
                }

                foreach (TCandidate candidate in _sequence.First().Item1)
                {
                    candidate.Predecessor = null;
                }
            }

            bool assert = (_k < 0 || _sequence.Count <= _k + 1);
            if (!assert)
            {
                throw new InvalidOperationException();
            }
        }

        protected void Remove(in TCandidate candidate, int index)
        {
            if (_sequence[index].Item3 == candidate)
            {
                return;
            }

            var vector = _sequence[index].Item1;
            _counters.Remove(candidate);
            vector.Remove(candidate);

            var predecessor = candidate.Predecessor;
            if (predecessor != null)
            {
                _counters[predecessor] = _counters[predecessor] - 1;
                if (_counters[predecessor] == 0)
                {
                    this.Remove(predecessor, index - 1);
                }
            }
        }

        public ICollection<TCandidate> Vector()
        {
            if (_sequence.Count == 0)
            {
                return new HashSet<TCandidate>();
            }
            else
            {
                return _sequence[_sequence.Count - 1].Item1;
            }
        }

        public TCandidate Estimate()
        {
            if (_sequence.Count == 0)
            {
                return null;
            }

            TCandidate estimate = null;
            foreach (TCandidate candidate in _sequence.Last().Item1)
            {
                if (estimate == null || candidate.Filtprob > estimate.Filtprob)
                {
                    estimate = candidate;
                }
            }
            return estimate;
        }


        /// <summary>
        /// Gets the most likely sequence of state candidates <i>s<sub>0</sub>, s<sub>1</sub>, ...,
        /// s<sub>t</sub></i>.
        /// </summary>
        /// <returns>List of the most likely sequence of state candidates.</returns>
        public IEnumerable<TCandidate> Sequence()
        {
            var ksequence = new Deque<TCandidate>(_sequence.Count);
            if (_sequence.Count > 0)
            {
                TCandidate kestimate = _sequence.Last().Item3;

                for (int i = _sequence.Count - 1; i >= 0; --i)
                {
                    if (kestimate != null)
                    {
                        ksequence.AddToFront(kestimate);
                        kestimate = kestimate.Predecessor;
                    }
                    else
                    {
                        ksequence.AddToFront(_sequence[i].Item3);
                        kestimate = _sequence[i].Item3.Predecessor;
                    }
                }
            }
            return ksequence;
        }

    }
}
