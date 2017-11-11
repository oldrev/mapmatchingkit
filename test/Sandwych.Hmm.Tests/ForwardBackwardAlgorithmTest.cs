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
using Xunit;

namespace Sandwych.Hmm.Tests
{

    public class ForwardBackwardAlgorithmTest
    {
        /**
         * Example taken from https://en.wikipedia.org/wiki/Forward%E2%80%93backward_algorithm.
         */
        [Fact]
        public void testForwardBackward()
        {
            List<Rain> candidates = new List<Rain>();
            candidates.Add(Rain.T);
            candidates.Add(Rain.F);

            var initialStateProbabilities = new Dictionary<Rain, double>();
            initialStateProbabilities.Add(Rain.T, 0.5);
            initialStateProbabilities.Add(Rain.F, 0.5);

            var emissionProbabilitiesForUmbrella = new Dictionary<Rain, double>();
            emissionProbabilitiesForUmbrella.Add(Rain.T, 0.9);
            emissionProbabilitiesForUmbrella.Add(Rain.F, 0.2);

            var emissionProbabilitiesForNoUmbrella = new Dictionary<Rain, double>();
            emissionProbabilitiesForNoUmbrella.Add(Rain.T, 0.1);
            emissionProbabilitiesForNoUmbrella.Add(Rain.F, 0.8);

            var transitionProbabilities = new Dictionary<Transition<Rain>, double>();
            transitionProbabilities.Add(new Transition<Rain>(Rain.T, Rain.T), 0.7);
            transitionProbabilities.Add(new Transition<Rain>(Rain.T, Rain.F), 0.3);
            transitionProbabilities.Add(new Transition<Rain>(Rain.F, Rain.T), 0.3);
            transitionProbabilities.Add(new Transition<Rain>(Rain.F, Rain.F), 0.7);

            ForwardBackwardAlgorithm<Rain, Umbrella> fw = new ForwardBackwardAlgorithm<Rain, Umbrella>();
            fw.StartWithInitialStateProbabilities(candidates, initialStateProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.F, candidates, emissionProbabilitiesForNoUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);
            fw.NextStep(Umbrella.T, candidates, emissionProbabilitiesForUmbrella,
                    transitionProbabilities);

            var result = fw.ComputeSmoothingProbabilities();
            Assert.Equal(6, result.Count);
            var DELTA = 4; //1e-4;
            Assert.Equal(0.6469, result[0][Rain.T], DELTA);
            Assert.Equal(0.3531, result[0][Rain.F], DELTA);
            Assert.Equal(0.8673, result[1][Rain.T], DELTA);
            Assert.Equal(0.1327, result[1][Rain.F], DELTA);
            Assert.Equal(0.8204, result[2][Rain.T], DELTA);
            Assert.Equal(0.1796, result[2][Rain.F], DELTA);
            Assert.Equal(0.3075, result[3][Rain.T], DELTA);
            Assert.Equal(0.6925, result[3][Rain.F], DELTA);
            Assert.Equal(0.8204, result[4][Rain.T], DELTA);
            Assert.Equal(0.1796, result[4][Rain.F], DELTA);
            Assert.Equal(0.8673, result[5][Rain.T], DELTA);
            Assert.Equal(0.1327, result[5][Rain.F], DELTA);
        }

    }

}
