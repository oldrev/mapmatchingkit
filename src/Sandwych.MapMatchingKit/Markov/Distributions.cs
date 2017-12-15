using System;
/**
 * Copyright (C) 2015-2016, BMW Car IT GmbH and BMW AG
 * Author: Stefan Holder (stefan.holder@bmw.de)
 * 
 * Copyright (C) 2017 Wei "oldrev" Li (oldrev@gmail.com)
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

namespace Sandwych.MapMatchingKit.Markov
{


    /// <summary>
    /// Implements various probability distributions.
    /// </summary>
    public static class Distributions
    {

        public static double NormalDistribution(in double sigma, in double x) =>
            1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma) * Math.Exp(-0.5 * Math.Pow(x / sigma, 2));

        /// <summary>
        /// Use this function instead of Math.log(normalDistribution(sigma, x)) to avoid an
        /// arithmetic underflow for very small probabilities.
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LogNormalDistribution(in double sigma, in double x) =>
            Math.Log(1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma)) + (-0.5 * Math.Pow(x / sigma, 2));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beta">beta =1/lambda with lambda being the standard exponential distribution rate parameter
        /// </param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ExponentialDistribution(in double beta, in double x) =>
            1.0 / beta * Math.Exp(-x / beta);

        /// <summary>
        /// Use this function instead of Math.log(exponentialDistribution(beta, x)) to avoid an
        /// arithmetic underflow for very small probabilities.
        /// 
        /// </summary>
        /// <param name="beta"> beta = 1 / lambda with lambda being the standard exponential distribution rate parameter
        /// </param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LogExponentialDistribution(in double beta, in double x) =>
            Math.Log(1.0 / beta) - (x / beta);
    }
}