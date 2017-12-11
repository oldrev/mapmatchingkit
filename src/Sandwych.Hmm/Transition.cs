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
        private readonly TState _fromCandidate;
        private readonly TState _toCandidate;

        public Transition(TState fromCandidate, TState toCandidate)
        {
            this._fromCandidate = fromCandidate;
            this._toCandidate = toCandidate;
        }

        public TState FromCandidate => _fromCandidate;

        public TState ToCandidate => _toCandidate;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _fromCandidate.GetHashCode();
                hash = hash * 31 + _toCandidate.GetHashCode();
                return hash;
            }
        }

        public bool Equals(Transition<TState> other) =>
            this._fromCandidate.Equals(other._fromCandidate) && this._toCandidate.Equals(other._toCandidate);

        public override String ToString()
        {
            return $"Transition [fromCandidate={_fromCandidate}, toCandidate={_toCandidate}]";
        }

    }
}
