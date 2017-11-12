/**
 * Copyright (C) 2015, BMW Car IT GmbH
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

namespace Sandwych.Hmm
{

    /// <summary>
    /// Implementation utilities.
    /// </summary>
    public static class HmmUtils
    {

        public static int InitialHashMapCapacity(int maxElements)
        {
            // Default load factor of HashMaps is 0.75
            return (int)(maxElements / 0.75) + 1;
        }

        public static Dictionary<TState, double> LogToNonLogProbabilities<TState>(this IReadOnlyDictionary<TState, double> logProbabilities)
        {
            var result = new Dictionary<TState, double>();
            foreach (var entry in logProbabilities)
            {
                result.Add(entry.Key, Math.Exp(entry.Value));
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="probability"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <remarks>Note that this check must not be used for probability densities. </remarks>
        public static bool ProbabilityInRange(this double probability, double delta)
        {
            return probability >= -delta && probability <= 1.0 + delta;
        }

        public static int CombineHashCodes(this IEnumerable<int> hashCodes)
        {
            int hash = 5381;

            foreach (var hashCode in hashCodes)
            {
                hash = ((hash << 5) + hash) ^ hashCode;
            }
            return hash;
        }

    }

}
