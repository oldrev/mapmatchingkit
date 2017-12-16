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
using System.Text;

namespace Sandwych.MapMatchingKit.Tests.Model
{
    /**
     * Default type to represent the position of a vehicle on the road network.
     * It is also possible to use a custom road position class instead.
     */
    public class RoadPosition : IEquatable<RoadPosition>
    {
        /**
         * ID of the edge, on which the vehicle is positioned.
         */
        public long EdgeId { get; }

        /**
         * Position on the edge from beginning as a number in the interval [0,1].
         */
        public double Fraction { get; }

        public Point Position { get; }

        public RoadPosition(long edgeId, double fraction, in Point position)
        {
            if (fraction < 0.0 || fraction > 1.0)
            {
                throw new InvalidOperationException();
            }

            this.EdgeId = edgeId;
            this.Fraction = fraction;
            this.Position = position;
        }

        public RoadPosition(long edgeId, double fraction, double x, double y) : this(edgeId, fraction, new Point(x, y))
        {
        }

        public override string ToString()
        {
            return "RoadPosition [edgeId=" + EdgeId + ", fraction=" + Fraction
                    + ", position=" + Position + "]";
        }

        public bool Equals(RoadPosition other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.EdgeId == other.EdgeId && this.Fraction == other.Fraction;

        }
    }
}
