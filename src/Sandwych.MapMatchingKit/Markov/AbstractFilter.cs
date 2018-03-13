using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandwych.MapMatchingKit.Utility;
using Microsoft.Extensions.Logging;

namespace Sandwych.MapMatchingKit.Markov
{

    /// <summary>
    /// Hidden Markov Model (HMM) filter for online and offline inference of states in a stochastic
    /// process.
    /// </summary>
    /// <typeparam name="TCandidate">Candidate implements <see cref="IStateCandidate{TCandidate, TTransition, TSample}"/>.</typeparam>
    /// <typeparam name="TTransition">Transition inherits from {@link Transition}.</typeparam>
    /// <typeparam name="TSample">Sample implements <see cref="ISample"/>.</typeparam>
    public abstract class AbstractFilter<TCandidate, TTransition, TSample> :
        IFilter<TCandidate, TTransition, TSample>
        where TCandidate : class, IStateCandidate<TCandidate, TTransition, TSample>
    {
        public readonly struct SampleCandidates
        {
            public TSample Sample { get; }
            public IEnumerable<TCandidate> Candidates { get; }

            public SampleCandidates(in TSample sample, IEnumerable<TCandidate> candidates)
            {
                this.Sample = sample;
                this.Candidates = candidates;
            }
        }

        public readonly struct SampleCandidate
        {
            public TSample Sample { get; }
            public TCandidate Candidate { get; }

            public SampleCandidate(in TSample sample, TCandidate candidate)
            {
                this.Sample = sample;
                this.Candidate = candidate;
            }
        }

        public readonly struct CandidateProbability
        {
            public TCandidate Candidate { get; }
            public double Probability { get; }

            public CandidateProbability(TCandidate cand, double prob)
            {
                this.Candidate = cand;
                this.Probability = prob;
            }
        }

        public readonly struct TransitionProbability
        {
            public TTransition Transition { get; }
            public double Probability { get; }

            public TransitionProbability(TTransition transition, double prob)
            {
                this.Transition = transition;
                this.Probability = prob;
            }
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        /// <summary>
        /// Gets state vector, which is a set of {@link StateCandidate} objects and with its emission
        /// probability.
        /// </summary>
        /// <param name="predecessors">Predecessor state candidate <i>s<sub>t-1</sub></i>.</param>
        /// <param name="sample">Measurement sample.</param>
        /// <returns>Set of tuples consisting of a {@link StateCandidate} and its emission probability.</returns>
        public abstract IEnumerable<CandidateProbability> ComputeCandidates(IEnumerable<TCandidate> predecessors, in TSample sample);


        /// <summary>
        /// Gets transition and its transition probability for a pair of {@link StateCandidate}s, which
        /// is a candidate <i>s<sub>t</sub></i> and its predecessor <i>s<sub>t</sub></i>
        /// </summary>
        /// <param name="predecessor">Tuple of predecessor state candidate <i>s<sub>t-1</sub></i> and its 
        /// respective measurement sample</param>
        /// <param name="candidate">Tuple of state candidate <i>s<sub>t</sub></i> and its respective measurement
        /// sample</param>
        /// <returns>
        /// Tuple consisting of the transition from <i>s<sub>t-1</sub></i> to
        /// <i>s<sub>t</sub></i> and its transition probability, or null if there is no
        /// transition.
        /// </returns>
        public abstract TransitionProbability ComputeTransition(in SampleCandidate predecessor, in SampleCandidate candidate);


        /// <summary>
        /// Gets transitions and its transition probabilities for each pair of state candidates
        /// <i>s<sub>t</sub></i> and <i>s<sub>t-1</sub></i>.
        ///
        /// <b>Note:</b> This method may be overridden for better performance, otherwise it defaults to
        /// the method {@link Filter#transition} for each single pair of state candidate and its possible
        /// predecessor.
        /// </summary>
        /// <param name="predecessors">Tuple of a set of predecessor state candidate <i>s<sub>t-1</sub></i> and 
        /// its respective measurement sample.</param>
        /// <param name="candidates">Tuple of a set of state candidate <i>s<sub>t</sub></i> and its respective 
        /// measurement sample.</param>
        /// <returns>
        /// Maps each predecessor state candidate <i>s<sub>t-1</sub> &#8712; S<sub>t-1</sub></i>
        /// to a map of state candidates <i>s<sub>t</sub> &#8712; S<sub>t</sub></i> containing
        /// all transitions from <i>s<sub>t-1</sub></i> to <i>s<sub>t</sub></i> and its
        /// transition probability, or null if there no transition.
        /// </returns>
        public virtual IDictionary<TCandidate, IDictionary<TCandidate, TransitionProbability>> ComputeTransitions(
            in SampleCandidates predecessors, in SampleCandidates candidates)
        {
            TSample sample = candidates.Sample;
            TSample previous = predecessors.Sample;

            var map = new Dictionary<TCandidate, IDictionary<TCandidate, TransitionProbability>>();

            foreach (TCandidate predecessor in predecessors.Candidates)
            {
                map.Add(predecessor, new Dictionary<TCandidate, TransitionProbability>());

                foreach (TCandidate candidate in candidates.Candidates)
                {
                    map[predecessor].Add(candidate, this.ComputeTransition(new SampleCandidate(previous, predecessor), new SampleCandidate(sample, candidate)));
                }
            }

            return map;
        }


        /// <summary>
        /// Executes Hidden Markov Model (HMM) filter iteration that determines for a given measurement
        /// sample <i>z<sub>t</sub></i>, which is a {@link Sample} object, and of a predecessor state
        /// vector <i>S<sub>t-1</sub></i>, which is a set of {@link StateCandidate} objects, a state
        /// vector <i>S<sub>t</sub></i> with filter and sequence probabilities set.
        ///
        /// <b>Note:</b> The set of state candidates <i>S<sub>t-1</sub></i> is allowed to be empty. This
        /// is either the initial case or an HMM break occured, which is no state candidates representing
        /// the measurement sample could be found.
        /// </summary>
        /// <param name="predecessors">State vector <i>S<sub>t-1</sub></i>, which may be empty.</param>
        /// <param name="previous">Previous measurement sample <i>z<sub>t-1</sub></i>.</param>
        /// <param name="sample">Measurement sample <i>z<sub>t</sub></i>.</param>
        /// <returns>State vector <i>S<sub>t</sub></i>, which may be empty if an HMM break occured.</returns>
        public virtual ICollection<TCandidate> Execute(IEnumerable<TCandidate> predecessors, in TSample previous, in TSample sample)
        {
            if (predecessors == null)
            {
                throw new ArgumentNullException(nameof(predecessors));
            }

            var result = new HashSet<TCandidate>();
            var candidates = this.ComputeCandidates(predecessors, sample);
            //logger.trace("{} state candidates", candidates.size());

            double normsum = 0;

            if (predecessors.Count() > 0)
            {
                var states = candidates.Select(c => c.Candidate).Distinct();

                var transitions = this.ComputeTransitions(
                    new SampleCandidates(previous, predecessors),
                    new SampleCandidates(sample, states));

                foreach (var candidate in candidates)
                {
                    var candidate_ = candidate.Candidate;
                    candidate_.Seqprob = Double.NegativeInfinity;

                    foreach (var predecessor in predecessors)
                    {
                        if (transitions[predecessor].TryGetValue(candidate_, out var transition))
                        {
                            //if (transition == null || transition.Item2 == 0)
                            if (transition.Probability == 0D)
                            {
                                continue;
                            }

                            candidate_.Filtprob += transition.Probability * predecessor.Filtprob;

                            var seqprob = predecessor.Seqprob + Math.Log10(transition.Probability) + Math.Log10(candidate.Probability);

                            if (seqprob > candidate_.Seqprob)
                            {
                                candidate_.Predecessor = predecessor;
                                candidate_.Transition = transition.Transition;
                                candidate_.Seqprob = seqprob;
                            }
                        }
                    }

                    if (candidate_.Filtprob == 0)
                    {
                        continue;
                    }

                    candidate_.Filtprob = candidate_.Filtprob * candidate.Probability;
                    result.Add(candidate_);

                    normsum += candidate_.Filtprob;
                }
            }

            if (this.Logger.IsEnabled(LogLevel.Debug) && candidates.Count() > 0 && result.Count == 0 && predecessors.Count() > 0)
            {
                this.Logger.LogDebug("HMM break - no state transitions");
            }

            if (result.Count == 0 || predecessors.Count() == 0)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.Probability == 0)
                    {
                        continue;
                    }
                    TCandidate candidate_ = candidate.Candidate;
                    normsum += candidate.Probability;
                    candidate_.Filtprob = candidate.Probability;
                    candidate_.Seqprob = Math.Log10(candidate.Probability);
                    result.Add(candidate_);
                }
            }

            if (this.Logger.IsEnabled(LogLevel.Debug) && result.Count == 0)
            {
                this.Logger.LogDebug("HMM break - no state emissions");
            }

            foreach (TCandidate candidate in result)
            {
                candidate.Filtprob = candidate.Filtprob / normsum;
            }

            if (this.Logger.IsEnabled(LogLevel.Trace))
            {
                this.Logger.LogTrace("{0} state candidates for state update", result.Count);
            }
            return result;
        }
    }
}
