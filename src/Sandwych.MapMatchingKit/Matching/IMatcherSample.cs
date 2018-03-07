using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Matching
{
    public interface IMatcherSample : ISample
    {
        long Id { get; }
        float Azimuth { get; }
        Coordinate2D Coordinate { get; }
        bool IsNaN { get; }
        bool HasAzimuth { get; }
    }
}
