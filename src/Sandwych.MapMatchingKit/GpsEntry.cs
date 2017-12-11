using System;
using System.Collections.Generic;
using System.Text;

using Sandwych.MapMatchingKit.Geometry;

namespace Sandwych.MapMatchingKit
{
    public struct GpsEntry
    {
        public Point2D Point { get; set; }
        public DateTimeOffset Time { get; set; } 
    }
}
