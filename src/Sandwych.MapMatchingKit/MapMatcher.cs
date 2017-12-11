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
using Sandwych.Hmm;
using Sandwych.MapMatchingKit.Util;

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
    public class MapMatcher<Path>
    {
        private double measurementErrorSigma = 0;
        private double transitionProbabilityBeta = 0;

        /// <summary>
        /// Computes the most likely candidate sequence for the GPX entries.
        /// </summary>
        /// <param name="timeSteps"></param>
        /// <param name="originalGpxEntriesCount"></param>
        /// <param name="queryGraph"></param>
        /// <returns></returns>
        private IReadOnlyList<SequenceState<MatcherState, GpsEntry, Path>> ComputeViterbiSequence(
                IEnumerable<TimeStep<MatcherState, GpsEntry, Path>> timeSteps, int originalGpxEntriesCount)
        {
            HmmProbabilities probabilities = new HmmProbabilities(measurementErrorSigma, transitionProbabilityBeta);
            var viterbi = new ViterbiAlgorithm<MatcherState, GpsEntry, Path>();

            int timeStepCounter = 0;
            TimeStep<MatcherState, GpsEntry, Path> prevTimeStep = null;
            foreach (var timeStep in timeSteps)
            {
                this.ComputeEmissionProbabilities(timeStep, probabilities);

                if (prevTimeStep == null)
                {
                    viterbi.StartWithInitialObservation(timeStep.Observation, timeStep.Candidates, timeStep.EmissionLogProbabilities);
                }
                else
                {
                    this.ComputeTransitionProbabilities(prevTimeStep, timeStep, probabilities);
                    viterbi.NextStep(timeStep.Observation, timeStep.Candidates,
                            timeStep.EmissionLogProbabilities, timeStep.TransitionLogProbabilities,
                            timeStep.RoadPaths);
                }
                if (viterbi.IsBroken())
                {
                    var likelyReasonStr = "";
                    if (prevTimeStep != null)
                    {
                        GpsEntry prevGPXE = prevTimeStep.Observation;
                        GpsEntry gpxe = timeStep.Observation;
                        double dist = 500.0; //distanceCalc.calcDist(prevGPXE.lat, prevGPXE.lon, gpxe.lat, gpxe.lon);
                        if (dist > 2000)
                        {
                            likelyReasonStr = "Too long distance to previous measurement? " + Math.Round(dist) + "m, ";
                        }
                    }

                    throw new InvalidOperationException();
                }

                timeStepCounter++;
                prevTimeStep = timeStep;
            }

            return viterbi.ComputeMostLikelySequence();
        }


        private void ComputeTransitionProbabilities(TimeStep<MatcherState, GpsEntry, Path> prevTimeStep,
                                            TimeStep<MatcherState, GpsEntry, Path> timeStep,
                                            HmmProbabilities probabilities)
        {
            double linearDistance = 0.1;// distanceCalc.calcDist(prevTimeStep.observation.lat,
                                        //prevTimeStep.observation.lon, timeStep.observation.lat, timeStep.observation.lon);

            // time difference in seconds
            double timeDiff = (timeStep.Observation.Time - prevTimeStep.Observation.Time).TotalSeconds;  // / 1000.0;

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


        private void ComputeEmissionProbabilities(TimeStep<MatcherState, GpsEntry, Path> timeStep, in HmmProbabilities probabilities)
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
