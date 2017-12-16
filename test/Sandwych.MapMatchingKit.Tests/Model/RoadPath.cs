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

namespace Sandwych.MapMatchingKit.Tests.Model
{


    /**
     * Represents the road path between two consecutive road positions.
     */
    public class RoadPath : IEquatable<RoadPath>
    {
        // The following members are used to check whether the correct road paths are retrieved
        // from the most likely sequence.
        public RoadPosition From { get; }
        public RoadPosition To { get; }

        public RoadPath(RoadPosition from, RoadPosition to)
        {
            this.From = from;
            this.To = to;
        }

        public bool Equals(RoadPath other)
        {
            return this.From.Equals(other.From) && this.To.Equals(other.To);
        }
    }
}
