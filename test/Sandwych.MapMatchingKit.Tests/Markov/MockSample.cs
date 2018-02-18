using Sandwych.MapMatchingKit.Markov;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Tests.Markov
{
    public sealed class MockSample : ISample
    {
        public DateTimeOffset Time { get; }

        public MockSample(long time)
        {
            this.Time = DateTimeOffset.MinValue.AddMilliseconds(time);
        }
    }
}
