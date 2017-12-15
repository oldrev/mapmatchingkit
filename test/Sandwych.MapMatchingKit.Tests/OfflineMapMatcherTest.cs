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
using System.Linq;
using Xunit;
using Sandwych.MapMatchingKit.Tests.Model;
using Sandwych.MapMatchingKit.Markov;
using Sandwych.Hmm;

namespace Sandwych.MapMatchingKit.Tests
{
    /**
     * This class demonstrate how to use the hmm-lib for map matching. The methods
     * of this class can be used as a template to implement map matching for an actual map.
     *
     * The test scenario is depicted in ./OfflineMapMatcherTest.png.
     * All road segments can be driven in both directions. The orientation of road segments
     * is needed to determine the fractions of a road positions.
     */
    public class OfflineMapMatcherTest
    {

        private readonly HmmProbabilities hmmProbabilities = new HmmProbabilities();

        private readonly IDictionary<GpsMeasurement, IEnumerable<RoadPosition>> candidateMap =
                new Dictionary<GpsMeasurement, IEnumerable<RoadPosition>>();

        private readonly IDictionary<Transition<RoadPosition>, Double> routeLengths = new Dictionary<Transition<RoadPosition>, double>();

        private readonly GpsMeasurement gps1 = new GpsMeasurement(Seconds(0), 10, 10);
        private readonly GpsMeasurement gps2 = new GpsMeasurement(Seconds(1), 30, 20);
        private readonly GpsMeasurement gps3 = new GpsMeasurement(Seconds(2), 30, 40);
        private readonly GpsMeasurement gps4 = new GpsMeasurement(Seconds(3), 10, 70);

        private readonly RoadPosition rp11 = new RoadPosition(1, 1.0 / 5.0, 20.0, 10.0);
        private readonly RoadPosition rp12 = new RoadPosition(2, 1.0 / 5.0, 60.0, 10.0);
        private readonly RoadPosition rp21 = new RoadPosition(1, 2.0 / 5.0, 20.0, 20.0);
        private readonly RoadPosition rp22 = new RoadPosition(2, 2.0 / 5.0, 60.0, 20.0);
        private readonly RoadPosition rp31 = new RoadPosition(1, 5.0 / 6.0, 20.0, 40.0);
        private readonly RoadPosition rp32 = new RoadPosition(3, 1.0 / 4.0, 30.0, 50.0);
        private readonly RoadPosition rp33 = new RoadPosition(2, 5.0 / 6.0, 60.0, 40.0);
        private readonly RoadPosition rp41 = new RoadPosition(4, 2.0 / 3.0, 20.0, 70.0);
        private readonly RoadPosition rp42 = new RoadPosition(5, 2.0 / 3.0, 60.0, 70.0);

        public OfflineMapMatcherTest()
        {
            candidateMap.Add(gps1, new RoadPosition[] { rp11, rp12 });
            candidateMap.Add(gps2, new RoadPosition[] { rp21, rp22 });
            candidateMap.Add(gps3, new RoadPosition[] { rp31, rp32, rp33 });
            candidateMap.Add(gps4, new RoadPosition[] { rp41, rp42 });

            AddRouteLength(rp11, rp21, 10.0);
            AddRouteLength(rp11, rp22, 110.0);
            AddRouteLength(rp12, rp21, 110.0);
            AddRouteLength(rp12, rp22, 10.0);

            AddRouteLength(rp21, rp31, 20.0);
            AddRouteLength(rp21, rp32, 40.0);
            AddRouteLength(rp21, rp33, 80.0);
            AddRouteLength(rp22, rp31, 80.0);
            AddRouteLength(rp22, rp32, 60.0);
            AddRouteLength(rp22, rp33, 20.0);

            AddRouteLength(rp31, rp41, 30.0);
            AddRouteLength(rp31, rp42, 70.0);
            AddRouteLength(rp32, rp41, 30.0);
            AddRouteLength(rp32, rp42, 50.0);
            AddRouteLength(rp33, rp41, 70.0);
            AddRouteLength(rp33, rp42, 30.0);
        }

        private static DateTimeOffset Seconds(int seconds)
        {
            return DateTimeOffset.MinValue.AddSeconds(seconds);
        }

        private void AddRouteLength(in RoadPosition from, in RoadPosition to, in double routeLength)
        {
            this.routeLengths.Add(new Transition<RoadPosition>(from, to), routeLength);
        }

        /*
         * Returns the Cartesian distance between two points.
         * For real map matching applications, one would compute the great circle distance between
         * two GPS points.
         */
        private double ComputeDistance(in Point p1, in Point p2)
        {
            double xDiff = p1.X - p2.X;
            double yDiff = p1.Y - p2.Y;
            return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }

        /*
         * For real map matching applications, candidates would be computed using a radius query.
         */
        private IEnumerable<RoadPosition> ComputeCandidates(GpsMeasurement gpsMeasurement)
        {
            return candidateMap[gpsMeasurement];
        }

        private void ComputeEmissionProbabilities(
                TimeStep<RoadPosition, GpsMeasurement, RoadPath> timeStep)
        {
            foreach (RoadPosition candidate in timeStep.Candidates)
            {
                double distance =
                        this.ComputeDistance(candidate.Position, timeStep.Observation.Position);
                timeStep.AddEmissionLogProbability(candidate,
                        hmmProbabilities.EmissionLogProbability(distance));
            }
        }

        private void ComputeTransitionProbabilities(
                TimeStep<RoadPosition, GpsMeasurement, RoadPath> prevTimeStep,
                TimeStep<RoadPosition, GpsMeasurement, RoadPath> timeStep)
        {
            double linearDistance = this.ComputeDistance(prevTimeStep.Observation.Position,
                    timeStep.Observation.Position);
            double timeDiff = timeStep.Observation.Time.ToUnixTimeSeconds() - prevTimeStep.Observation.Time.ToUnixTimeSeconds();

            foreach (RoadPosition from in prevTimeStep.Candidates)
            {
                foreach (RoadPosition to in timeStep.Candidates)
                {

                    // For real map matching applications, route lengths and road paths would be
                    // computed using a router. The most efficient way is to use a single-source
                    // multi-target router.
                    double routeLength = routeLengths[new Transition<RoadPosition>(from, to)];
                    timeStep.AddRoadPath(from, to, new RoadPath(from, to));

                    double transitionLogProbability = hmmProbabilities.TransitionLogProbability(
                            routeLength, linearDistance, timeDiff);
                    timeStep.AddTransitionLogProbability(from, to, transitionLogProbability);
                }
            }
        }

        [Fact]
        public void TestMapMatching()
        {
            List<GpsMeasurement> gpsMeasurements = new List<GpsMeasurement>() { gps1, gps2, gps3, gps4 };

            var viterbi = new ViterbiModel<RoadPosition, GpsMeasurement, RoadPath>();
            TimeStep<RoadPosition, GpsMeasurement, RoadPath> prevTimeStep = default;
            foreach (var gpsMeasurement in gpsMeasurements)
            {
                var candidates = this.ComputeCandidates(gpsMeasurement);
                var timeStep = new TimeStep<RoadPosition, GpsMeasurement, RoadPath>(gpsMeasurement, candidates);
                this.ComputeEmissionProbabilities(timeStep);
                if (prevTimeStep == null)
                {
                    viterbi.Start(timeStep.Observation, timeStep.Candidates, timeStep.EmissionLogProbabilities);
                }
                else
                {
                    this.ComputeTransitionProbabilities(prevTimeStep, timeStep);
                    viterbi.NextStep(timeStep.Observation, timeStep.Candidates,
                            timeStep.EmissionLogProbabilities, timeStep.TransitionLogProbabilities,
                            timeStep.RoadPaths);
                }
                prevTimeStep = timeStep;
            }

            var roadPositions =
                    viterbi.ComputeMostLikelySequence();

            Assert.False(viterbi.IsBroken);
            var expected = new SequenceState<RoadPosition, GpsMeasurement, RoadPath>[] {
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(rp11, gps1, null),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(rp21, gps2, new RoadPath(rp11, rp21)),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(rp31, gps3, new RoadPath(rp21, rp31)),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(rp41, gps4, new RoadPath(rp31, rp41))
            };
            Assert.Equal(expected[0], roadPositions[0]);
            Assert.Equal(expected[1], roadPositions[1]);
            Assert.Equal(expected[2], roadPositions[2]);
            Assert.Equal(expected[3], roadPositions[3]);
            Assert.Equal(expected.AsEnumerable(), roadPositions.AsEnumerable());
        }

    }
}
