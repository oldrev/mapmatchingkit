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

using Sandwych.MapMatchingKit.Markov;
using System;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests
{
    public class DistributionsTest
    {
        //private static double Delta = 1e-8;
        private const int Precision = 8;


        [Fact]
        public void TestLogNormalDistribution()
        {
            Assert.Equal(Math.Log(Distributions.NormalDistribution(5, 6)),
                    Distributions.LogNormalDistribution(5, 6), Precision);
        }

        [Fact]
        public void TestLogExponentialDistribution()
        {
            Assert.Equal(Math.Log(Distributions.ExponentialDistribution(5, 6)),
                    Distributions.LogExponentialDistribution(5, 6), Precision);
        }

    }
}
