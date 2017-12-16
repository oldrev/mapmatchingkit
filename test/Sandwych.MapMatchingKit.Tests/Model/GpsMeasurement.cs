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
     * Example type for location coordinates.
     */
    public readonly struct GpsMeasurement
    {

        public long Time { get; }

        public Point Position { get; }

        public GpsMeasurement(in long time, in Point position)
        {
            this.Time = time;
            this.Position = position;
        }

        public GpsMeasurement(in long time, in double lon, in double lat) : this(time, new Point(lon, lat))
        {
        }

        public override String ToString()
        {
            return "GpsMeasurement [time=" + this.Time + ", position=" + this.Position + "]";
        }

    }
}
