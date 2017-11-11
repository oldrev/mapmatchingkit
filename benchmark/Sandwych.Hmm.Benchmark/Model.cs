using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Hmm.Benchmark
{
    public struct Rain
    {
        public readonly static Rain T = new Rain("Rain");
        public readonly static Rain F = new Rain("Sun");

        private readonly string _value;

        public Rain(string value)
        {
            _value = value;
        }

        public override String ToString() => _value;
        public override int GetHashCode() => _value.GetHashCode();

    }

    public class Umbrella
    {
        public readonly static Umbrella T = new Umbrella("Umbrella");
        public readonly static Umbrella F = new Umbrella("No umbrella");

        private readonly string _value;

        public Umbrella(string value)
        {
            _value = value;
        }

        public override String ToString() => _value;
        public override int GetHashCode() => _value.GetHashCode();
    }

    public struct Descriptor
    {
        public readonly static Descriptor R2R = new Descriptor("R2R");
        public readonly static Descriptor R2S = new Descriptor("R2S");
        public readonly static Descriptor S2R = new Descriptor("S2R");
        public readonly static Descriptor S2S = new Descriptor("S2S");

        private readonly string _value;
        public Descriptor(string value)
        {
            _value = value;
        }

        public override String ToString() => _value;
        public override int GetHashCode() => _value.GetHashCode();

    }
}
