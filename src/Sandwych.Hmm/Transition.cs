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

namespace Sandwych.Hmm
{

    /// <summary>
    /// Represents the transition between two consecutive candidates.
    /// </summary>
    /// <typeparam name="TState">the state type</typeparam>
    public readonly struct Transition<TState> : IEquatable<Transition<TState>>
    {
        public TState FromCandidate { get; }

        public TState ToCandidate { get; }

        public Transition(in TState fromCandidate, in TState toCandidate)
        {
            this.FromCandidate = fromCandidate;
            this.ToCandidate = toCandidate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + this.FromCandidate.GetHashCode();
                hash = hash * 31 + this.ToCandidate.GetHashCode();
                return hash;
            }
        }

        public bool Equals(Transition<TState> other) =>
            this.FromCandidate.Equals(other.FromCandidate) && this.ToCandidate.Equals(other.ToCandidate);

        public override String ToString()
        {
            return $"Transition [from={this.FromCandidate}, to={this.ToCandidate}]";
        }

    }
}
