using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Roads
{
    public interface IRoadReader : IDisposable
    {
        bool IsOpen { get; }
        void Open();
        void Close();
        RoadInfo Next();
    }
}
