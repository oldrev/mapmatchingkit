using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    public class BadGraphPathException : Exception
    {
        public BadGraphPathException() : base()
        {

        }

        public BadGraphPathException(string message) : base(message)
        {

        }
    }
}
