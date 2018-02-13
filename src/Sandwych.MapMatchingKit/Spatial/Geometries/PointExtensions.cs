using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class PointExtensions
    {
        public static Coordinate2D ToCoordinate2D(this IPoint self) =>
            new Coordinate2D(self.X, self.Y);
    }
}
