using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Markov
{
    public class Sample : ISample
    {
        public long Time { get; private set; }

        public Sample(long time)
        {
            this.Time = time;
        }
    }
}
