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
    /// <summary>
    /// This class demonstrate how to use the hmm-lib for map matching. The methods
    /// of this class can be used as a template to implement map matching for an actual map. <br/>

    /// The test scenario is depicted in ./OfflineMapMatcherTest.png. <br/>
    /// All road segments can be driven in both directions. The orientation of road segments
    /// is needed to determine the fractions of a road positions.
    /// </summary>
    public class OfflineMapMatcherTest
    {

        private readonly HmmProbabilities hmmProbabilities = new HmmProbabilities();

        private readonly IReadOnlyDictionary<GpsMeasurement, IEnumerable<RoadPosition>> _candidateMap =
                new Dictionary<GpsMeasurement, IEnumerable<RoadPosition>>();

        private readonly IReadOnlyDictionary<Transition<RoadPosition>, Double> _routeLengths;

        private GpsMeasurement Gps1 { get; } = new GpsMeasurement(Seconds(0), 10, 10);
        private GpsMeasurement Gps2 { get; } = new GpsMeasurement(Seconds(1), 30, 20);
        private GpsMeasurement Gps3 { get; } = new GpsMeasurement(Seconds(2), 30, 40);
        private GpsMeasurement Gps4 { get; } = new GpsMeasurement(Seconds(3), 10, 70);

        private RoadPosition RP11 { get; } = new RoadPosition(1, 1.0 / 5.0, 20.0, 10.0);
        private RoadPosition RP12 { get; } = new RoadPosition(2, 1.0 / 5.0, 60.0, 10.0);
        private RoadPosition RP21 { get; } = new RoadPosition(1, 2.0 / 5.0, 20.0, 20.0);
        private RoadPosition RP22 { get; } = new RoadPosition(2, 2.0 / 5.0, 60.0, 20.0);
        private RoadPosition RP31 { get; } = new RoadPosition(1, 5.0 / 6.0, 20.0, 40.0);
        private RoadPosition RP32 { get; } = new RoadPosition(3, 1.0 / 4.0, 30.0, 50.0);
        private RoadPosition RP33 { get; } = new RoadPosition(2, 5.0 / 6.0, 60.0, 40.0);
        private RoadPosition RP41 { get; } = new RoadPosition(4, 2.0 / 3.0, 20.0, 70.0);
        private RoadPosition RP42 { get; } = new RoadPosition(5, 2.0 / 3.0, 60.0, 70.0);

        public OfflineMapMatcherTest()
        {
            _candidateMap = new Dictionary<GpsMeasurement, IEnumerable<RoadPosition>>() {
                { Gps1, new RoadPosition[] { RP11, RP12 } },
                { Gps2, new RoadPosition[] { RP21, RP22 } },
                { Gps3, new RoadPosition[] { RP31, RP32, RP33 } },
                { Gps4, new RoadPosition[] { RP41, RP42 } },
            };

            var routeLengths = new Dictionary<Transition<RoadPosition>, double>();
            AddRouteLength(routeLengths, RP11, RP21, 10.0);
            AddRouteLength(routeLengths, RP11, RP22, 110.0);
            AddRouteLength(routeLengths, RP12, RP21, 110.0);
            AddRouteLength(routeLengths, RP12, RP22, 10.0);

            AddRouteLength(routeLengths, RP21, RP31, 20.0);
            AddRouteLength(routeLengths, RP21, RP32, 40.0);
            AddRouteLength(routeLengths, RP21, RP33, 80.0);
            AddRouteLength(routeLengths, RP22, RP31, 80.0);
            AddRouteLength(routeLengths, RP22, RP32, 60.0);
            AddRouteLength(routeLengths, RP22, RP33, 20.0);

            AddRouteLength(routeLengths, RP31, RP41, 30.0);
            AddRouteLength(routeLengths, RP31, RP42, 70.0);
            AddRouteLength(routeLengths, RP32, RP41, 30.0);
            AddRouteLength(routeLengths, RP32, RP42, 50.0);
            AddRouteLength(routeLengths, RP33, RP41, 70.0);
            AddRouteLength(routeLengths, RP33, RP42, 30.0);
            this._routeLengths = routeLengths;
        }

        private static long Seconds(int seconds) =>
            DateTimeOffset.MinValue.AddSeconds(seconds).ToUnixTimeMilliseconds();

        private static void AddRouteLength(in IDictionary<Transition<RoadPosition>, double> routeLengths,
                                           in RoadPosition from, in RoadPosition to, in double routeLength) =>
            routeLengths.Add(new Transition<RoadPosition>(from, to), routeLength);

        /// <summary>
        /// Returns the Cartesian distance between two points. <br/>
        /// For real map matching applications, one would compute the great circle distance between
        /// two GPS points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double ComputeDistance(in Point p1, in Point p2)
        {
            double xDiff = p1.X - p2.X;
            double yDiff = p1.Y - p2.Y;
            return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }

        /// <summary>
        /// For real map matching applications, candidates would be computed using a radius query.
        /// </summary>
        private IEnumerable<RoadPosition> ComputeCandidates(in GpsMeasurement gpsMeasurement) =>
            _candidateMap[gpsMeasurement];

        private void ComputeEmissionProbabilities(
                in TimeStep<RoadPosition, GpsMeasurement, RoadPath> timeStep)
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
                in TimeStep<RoadPosition, GpsMeasurement, RoadPath> prevTimeStep,
                in TimeStep<RoadPosition, GpsMeasurement, RoadPath> timeStep)
        {
            var linearDistance = this.ComputeDistance(prevTimeStep.Observation.Position,
                    timeStep.Observation.Position);
            var timeDiff = (double)(timeStep.Observation.Time - prevTimeStep.Observation.Time) / 1000.0;

            foreach (var from in prevTimeStep.Candidates)
            {
                foreach (var to in timeStep.Candidates)
                {

                    // For real map matching applications, route lengths and road paths would be
                    // computed using a router. The most efficient way is to use a single-source
                    // multi-target router.
                    var routeLength = _routeLengths[new Transition<RoadPosition>(from, to)];
                    timeStep.AddRoadPath(from, to, new RoadPath(from, to));

                    var transitionLogProbability = hmmProbabilities.TransitionLogProbability(
                            routeLength, linearDistance, timeDiff);
                    timeStep.AddTransitionLogProbability(from, to, transitionLogProbability);
                }
            }
        }

        [Fact]
        public void TestMapMatching()
        {
            var gpsMeasurements = new List<GpsMeasurement>() { Gps1, Gps2, Gps3, Gps4 };

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
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(RP11, Gps1, null),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(RP21, Gps2, new RoadPath(RP11, RP21)),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(RP31, Gps3, new RoadPath(RP21, RP31)),
                new SequenceState<RoadPosition, GpsMeasurement, RoadPath>(RP41, Gps4, new RoadPath(RP31, RP41))
            };

            Assert.Equal(expected.Length, roadPositions.Count);

            for (int i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var rp = roadPositions[i];
                Assert.Equal(e.State.EdgeId, rp.State.EdgeId);
                Assert.Equal(e.State.Fraction, rp.State.Fraction, 8);
            }
        }

    }
}
