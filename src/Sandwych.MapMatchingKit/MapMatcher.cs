/*
 *  Licensed to GraphHopper GmbH under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  GraphHopper GmbH licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Operation.Distance;
using Sandwych.Hmm;
using Sandwych.MapMatchingKit.Markov;

namespace Sandwych.MapMatchingKit
{
    /**
     * This class matches real world GPX entries to the digital road network stored
     * in GraphHopper. The Viterbi algorithm is used to compute the most likely
     * sequence of map matching candidates. The Viterbi algorithm takes into account
     * the distance between GPX entries and map matching candidates as well as the
     * routing distances between consecutive map matching candidates.
     * <p>
     * <p>
     * See http://en.wikipedia.org/wiki/Map_matching and Newson, Paul, and John
     * Krumm. "Hidden Markov map matching through noise and sparseness." Proceedings
     * of the 17th ACM SIGSPATIAL International Conference on Advances in Geographic
     * Information Systems. ACM, 2009.
     *
     * @author Peter Karich
     * @author Michael Zilske
     * @author Stefan Holder
     * @author kodonnell
     */
    public class MapMatcher<TRoadPath>
    {
        private double measurementErrorSigma = 50.0;
        private double transitionProbabilityBeta = 2.0;


        /// <summary>
        /// Computes the most likely candidate sequence for the GPX entries.
        /// </summary>
        /// <param name="timeSteps"></param>
        /// <param name="originalGpsEntriesCount"></param>
        /// <param name="queryGraph"></param>
        /// <returns></returns>
        private IReadOnlyList<SequenceState<MatcherCandidate, TrajectoryEntry, TRoadPath>> ComputeViterbiSequence(
                in IEnumerable<TimeStep<MatcherCandidate, TrajectoryEntry, TRoadPath>> timeSteps, in int originalGpsEntriesCount)
        {
            var probabilities = new HmmProbabilities(measurementErrorSigma, transitionProbabilityBeta);
            var viterbi = new ViterbiModel<MatcherCandidate, TrajectoryEntry, TRoadPath>();

            int timeStepCounter = 0;
            TimeStep<MatcherCandidate, TrajectoryEntry, TRoadPath> prevTimeStep = default;
            bool hasPrevTimeStep = false;
            foreach (var timeStep in timeSteps)
            {
                this.ComputeEmissionProbabilities(timeStep, probabilities);

                if (!hasPrevTimeStep)
                {
                    viterbi.Start(timeStep.Observation, timeStep.Candidates, timeStep.EmissionLogProbabilities);
                }
                else
                {
                    this.ComputeTransitionProbabilities(prevTimeStep, timeStep, probabilities);
                    viterbi.NextStep(timeStep.Observation, timeStep.Candidates,
                            timeStep.EmissionLogProbabilities, timeStep.TransitionLogProbabilities,
                            timeStep.RoadPaths);
                }
                if (viterbi.IsBroken)
                {
                    var likelyReasonStr = "";
                    if (!hasPrevTimeStep)
                    {
                        var prevGPXE = prevTimeStep.Observation;
                        var gpxe = timeStep.Observation;
                        var distance = DistanceOp.Distance(prevGPXE.Point, gpxe.Point); //distanceCalc.calcDist(prevGPXE.lat, prevGPXE.lon, gpxe.lat, gpxe.lon);
                        if (distance > 2000)
                        {
                            likelyReasonStr = "Too long distance to previous measurement? " + Math.Round(distance) + "m, ";
                        }
                    }

                    throw new InvalidOperationException();
                }

                timeStepCounter++;
                prevTimeStep = timeStep;
            }

            return viterbi.ComputeMostLikelySequence();
        }


        private void ComputeTransitionProbabilities(in TimeStep<MatcherCandidate, TrajectoryEntry, TRoadPath> prevTimeStep,
                                                    in TimeStep<MatcherCandidate, TrajectoryEntry, TRoadPath> timeStep,
                                                    in HmmProbabilities probabilities)
        {
            double linearDistance = DistanceOp.Distance(prevTimeStep.Observation.Point, timeStep.Observation.Point);
            // distanceCalc.calcDist(prevTimeStep.observation.lat,
            //prevTimeStep.observation.lon, timeStep.observation.lat, timeStep.observation.lon);

            // time difference in seconds
            TimeSpan timeDiff = timeStep.Observation.Time - prevTimeStep.Observation.Time;  // / 1000.0;

            foreach (var from in prevTimeStep.Candidates)
            {
                foreach (var to in timeStep.Candidates)
                {
                    // enforce heading if required:
                    if (from.IsDirected)
                    {
                        // Make sure that the path starting at the "from" candidate goes through
                        // the outgoing edge.
                        /*
                        queryGraph.unfavorVirtualEdgePair(from.getQueryResult().getClosestNode(),
                                from.getIncomingVirtualEdge().getEdge());
                                */
                    }
                    if (to.IsDirected)
                    {
                        // Make sure that the path ending at "to" candidate goes through
                        // the incoming edge.
                        /*
                        queryGraph.unfavorVirtualEdgePair(to.getQueryResult().getClosestNode(),
                                to.getOutgoingVirtualEdge().getEdge());
                                */
                    }

                    // Need to create a new routing algorithm for every routing.
                    /*
                    var algo = algoFactory.createAlgo(queryGraph, algoOptions);

                    var path = algo.calcPath(from.getQueryResult().getClosestNode(), to.getQueryResult().getClosestNode());

                    if (path.isFound())
                    {
                        timeStep.AddRoadPath(from, to, path);

                        // The router considers unfavored virtual edges using edge penalties
                        // but this is not reflected in the path distance. Hence, we need to adjust the
                        // path distance accordingly.
                        double penalizedPathDistance = penalizedPathDistance(path,
                                queryGraph.getUnfavoredVirtualEdges());

                        double transitionLogProbability = probabilities
                                .TransitionLogProbability(penalizedPathDistance, linearDistance);
                        timeStep.AddTransitionLogProbability(from, to, transitionLogProbability);
                    }
                    else
                    {
                        //logger.debug("No path found for from: {}, to: {}", from, to);
                    }
                    //queryGraph.clearUnfavoredStatus();
                    */

                }
            }
        }


        private void ComputeEmissionProbabilities(in TimeStep<MatcherCandidate, TrajectoryEntry, TRoadPath> timeStep, in HmmProbabilities probabilities)
        {
            foreach (var candidate in timeStep.Candidates)
            {
                // road distance difference in meters
                var distance = 1.0; //candidate.getQueryResult().getQueryDistance();
                timeStep.AddEmissionLogProbability(candidate, probabilities.EmissionLogProbability(distance));
            }
        }

    }
}
