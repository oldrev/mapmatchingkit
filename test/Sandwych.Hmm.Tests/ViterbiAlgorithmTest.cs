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
using Xunit;

namespace Sandwych.Hmm.Tests
{
    public class ViterbiAlgorithmTest
    {

        private static int DELTA = 8;

        private List<Rain> GetStates(List<SequenceState<Rain, Umbrella, Descriptor>> sequenceStates)
        {
            var result = new List<Rain>();
            foreach (SequenceState<Rain, Umbrella, Descriptor> ss in sequenceStates)
            {
                result.Add(ss.State);
            }
            return result;
        }

        /**
         * Tests the Viterbi algorithms with the umbrella example taken from Russell, Norvig: Aritifical
         * Intelligence - A Modern Approach, 3rd edition, chapter 15.2.3. Note that the probabilities in
         * Figure 15.5 are different, since the book uses initial probabilities and the probabilities
         * for message m1:1 are normalized (not wrong but unnecessary).
         */
        [Fact]
        public void TestComputeMostLikelySequence()
        {
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var emissionLogProbabilitiesForUmbrella = new Dictionary<Rain, double>();
            emissionLogProbabilitiesForUmbrella[Rain.T] = Math.Log(0.9);
            emissionLogProbabilitiesForUmbrella[Rain.F] = Math.Log(0.2);

            var emissionLogProbabilitiesForNoUmbrella = new Dictionary<Rain, double>();
            emissionLogProbabilitiesForNoUmbrella[Rain.T] = Math.Log(0.1);
            emissionLogProbabilitiesForNoUmbrella[Rain.F] = Math.Log(0.8);

            var transitionLogProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.T)] = Math.Log(0.7);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.F)] = Math.Log(0.3);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.T)] = Math.Log(0.3);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.F)] = Math.Log(0.7);

            var transitionDescriptors = new Dictionary<Transition<Rain>, Descriptor>();
            transitionDescriptors[new Transition<Rain>(Rain.T, Rain.T)] = Descriptor.R2R;
            transitionDescriptors[new Transition<Rain>(Rain.T, Rain.F)] = Descriptor.R2S;
            transitionDescriptors[new Transition<Rain>(Rain.F, Rain.T)] = Descriptor.S2R;
            transitionDescriptors[new Transition<Rain>(Rain.F, Rain.F)] = Descriptor.S2S;

            var viterbi = (new ViterbiAlgorithm<Rain, Umbrella, Descriptor>()).SetKeepMessageHistory(true).
                    SetComputeSmoothingProbabilities(true);
            viterbi.StartWithInitialObservation(Umbrella.T, candidates,
                    emissionLogProbabilitiesForUmbrella);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilitiesForUmbrella,
                    transitionLogProbabilities, transitionDescriptors);
            viterbi.NextStep(Umbrella.F, candidates, emissionLogProbabilitiesForNoUmbrella,
                    transitionLogProbabilities, transitionDescriptors);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilitiesForUmbrella,
                    transitionLogProbabilities, transitionDescriptors);

            var result =
                     viterbi.computeMostLikelySequence();

            // Check most likely sequence
            Assert.Equal(4, result.Count);
            Assert.Equal(Rain.T, result[0].State);
            Assert.Equal(Rain.T, result[1].State);
            Assert.Equal(Rain.F, result[2].State);
            Assert.Equal(Rain.T, result[3].State);

            Assert.Equal(Umbrella.T, result[0].Observation);
            Assert.Equal(Umbrella.T, result[1].Observation);
            Assert.Equal(Umbrella.F, result[2].Observation);
            Assert.Equal(Umbrella.T, result[3].Observation);

            //Assert.Equal(null, result[0].TransitionDescriptor);
            Assert.Equal(Descriptor.R2R, result[1].TransitionDescriptor);
            Assert.Equal(Descriptor.R2S, result[2].TransitionDescriptor);
            Assert.Equal(Descriptor.S2R, result[3].TransitionDescriptor);

            // Check for HMM breaks
            Assert.False(viterbi.IsBroken());

            // Check message history
            var expectedMessageHistory = new List<IDictionary<Rain, double>>();
            var message = new Dictionary<Rain, double>();
            message[Rain.T] = 0.9;
            message[Rain.F] = 0.2;
            expectedMessageHistory.Add(message);

            message = new Dictionary<Rain, double>();
            message[Rain.T] = 0.567;
            message[Rain.F] = 0.054;
            expectedMessageHistory.Add(message);

            message = new Dictionary<Rain, double>();
            message[Rain.T] = 0.03969;
            message[Rain.F] = 0.13608;
            expectedMessageHistory.Add(message);

            message = new Dictionary<Rain, double>();
            message[Rain.T] = 0.0367416;
            message[Rain.F] = 0.0190512;
            expectedMessageHistory.Add(message);

            var actualMessageHistory = viterbi.MessageHistory;
            CheckMessageHistory(expectedMessageHistory, actualMessageHistory);
        }

        [Fact]
        public void TestSetParams()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();

            Assert.False(viterbi.IsKeepMessageHistory);
            viterbi.SetKeepMessageHistory(true);
            Assert.True(viterbi.IsKeepMessageHistory);
            viterbi.SetKeepMessageHistory(false);
            Assert.False(viterbi.IsKeepMessageHistory);

            Assert.False(viterbi.IsComputeSmoothingProbabilities);
            viterbi.SetComputeSmoothingProbabilities(true);
            Assert.True(viterbi.IsComputeSmoothingProbabilities);
            viterbi.SetComputeSmoothingProbabilities(false);
            Assert.False(viterbi.IsComputeSmoothingProbabilities);
        }

        private void CheckMessageHistory(IList<IDictionary<Rain, double>> expectedMessageHistory,
                IList<IDictionary<Rain, double>> actualMessageHistory)
        {
            Assert.Equal(expectedMessageHistory.Count, actualMessageHistory.Count);
            for (int i = 0; i < expectedMessageHistory.Count; i++)
            {
                CheckMessage(expectedMessageHistory[i], actualMessageHistory[i]);
            }
        }

        private void CheckMessage(IDictionary<Rain, Double> expectedMessage, IDictionary<Rain, Double> actualMessage)
        {
            Assert.Equal(expectedMessage.Count, actualMessage.Count);
            foreach (var entry in expectedMessage)
            {
                Assert.Equal(entry.Value, Math.Exp(actualMessage[entry.Key]), DELTA);
            }
        }

        [Fact]
        public void TestEmptySequence()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            var result = viterbi.computeMostLikelySequence();

            //Assert.Equal(Arrays.asList(), result);
            Assert.False(viterbi.IsBroken());
        }

        [Fact]
        public void TestBreakAtInitialMessage()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var emissionLogProbabilities = new Dictionary<Rain, double>();
            emissionLogProbabilities[Rain.T] = Math.Log(0.0);
            emissionLogProbabilities[Rain.F] = Math.Log(0.0);
            viterbi.StartWithInitialObservation(Umbrella.T, candidates, emissionLogProbabilities);
            Assert.True(viterbi.IsBroken());
            //Assert.Equal(Arrays.asList(), viterbi.computeMostLikelySequence());
        }

        [Fact]
        public void TestEmptyInitialMessage()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            viterbi.StartWithInitialObservation(Umbrella.T, new List<Rain>(), new Dictionary<Rain, Double>());
            Assert.True(viterbi.IsBroken());
            //assertEquals(Arrays.asList(), viterbi.computeMostLikelySequence());
        }

        [Fact]
        public void TestBreakAtFirstTransition()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var emissionLogProbabilities = new Dictionary<Rain, double>();
            emissionLogProbabilities[Rain.T] = Math.Log(0.9);
            emissionLogProbabilities[Rain.F] = Math.Log(0.2);
            viterbi.StartWithInitialObservation(Umbrella.T, candidates, emissionLogProbabilities);
            Assert.False(viterbi.IsBroken());

            var transitionLogProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.T)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.F)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.T)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.F)] = Math.Log(0.0);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilities,
                    transitionLogProbabilities);

            Assert.True(viterbi.IsBroken());
            //assertEquals(Arrays.asList(Rain.T), states(viterbi.computeMostLikelySequence()));
        }

        [Fact]
        public void TestBreakAtFirstTransitionWithNoCandidates()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var emissionLogProbabilities = new Dictionary<Rain, double>();
            emissionLogProbabilities[Rain.T] = Math.Log(0.9);
            emissionLogProbabilities[Rain.F] = Math.Log(0.2);
            viterbi.StartWithInitialObservation(Umbrella.T, candidates, emissionLogProbabilities);
            Assert.False(viterbi.IsBroken());

            viterbi.NextStep(Umbrella.T, new List<Rain>(), new Dictionary<Rain, Double>(),
                    new Dictionary<Transition<Rain>, Double>());
            Assert.True(viterbi.IsBroken());

            //assertEquals(Arrays.asList(Rain.T), states(viterbi.computeMostLikelySequence()));
        }

        [Fact]
        public void TestBreakAtSecondTransition()
        {
            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var emissionLogProbabilities = new Dictionary<Rain, double>();
            emissionLogProbabilities[Rain.T] = Math.Log(0.9);
            emissionLogProbabilities[Rain.F] = Math.Log(0.2);
            viterbi.StartWithInitialObservation(Umbrella.T, candidates, emissionLogProbabilities);
            Assert.False(viterbi.IsBroken());

            var transitionLogProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.T)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.F)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.T)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.F)] = Math.Log(0.5);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilities,
                    transitionLogProbabilities);
            Assert.False(viterbi.IsBroken());

            transitionLogProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.T)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.F)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.T)] = Math.Log(0.0);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.F)] = Math.Log(0.0);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilities,
                    transitionLogProbabilities);

            Assert.True(viterbi.IsBroken());
            //assertEquals(Arrays.asList(Rain.T, Rain.T), states(viterbi.computeMostLikelySequence()));
        }

        [Fact]
        /**
         * Checks if the first candidate is returned if multiple candidates are equally likely.
         */
        public void TestDeterministicCandidateOrder()
        {
            var candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            // Reverse usual order of emission and transition probabilities keys since their order
            // should not matter.
            var emissionLogProbabilitiesForUmbrella = new Dictionary<Rain, double>();
            emissionLogProbabilitiesForUmbrella[Rain.F] = Math.Log(0.5);
            emissionLogProbabilitiesForUmbrella[Rain.T] = Math.Log(0.5);

            var emissionLogProbabilitiesForNoUmbrella = new Dictionary<Rain, double>();
            emissionLogProbabilitiesForNoUmbrella[Rain.F] = Math.Log(0.5);
            emissionLogProbabilitiesForNoUmbrella[Rain.T] = Math.Log(0.5);

            var transitionLogProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.T)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.F, Rain.F)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.T)] = Math.Log(0.5);
            transitionLogProbabilities[new Transition<Rain>(Rain.T, Rain.F)] = Math.Log(0.5);

            var viterbi = new ViterbiAlgorithm<Rain, Umbrella, Descriptor>();
            viterbi.StartWithInitialObservation(Umbrella.T, candidates,
                    emissionLogProbabilitiesForUmbrella);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilitiesForUmbrella,
                    transitionLogProbabilities);
            viterbi.NextStep(Umbrella.F, candidates, emissionLogProbabilitiesForNoUmbrella,
                    transitionLogProbabilities);
            viterbi.NextStep(Umbrella.T, candidates, emissionLogProbabilitiesForUmbrella,
                    transitionLogProbabilities);

            var result = viterbi.computeMostLikelySequence();

            // Check most likely sequence
            Assert.Equal(4, result.Count);
            Assert.Equal(Rain.T, result[0].State);
            Assert.Equal(Rain.T, result[1].State);
            Assert.Equal(Rain.T, result[2].State);
            Assert.Equal(Rain.T, result[3].State);
        }

    }

}
