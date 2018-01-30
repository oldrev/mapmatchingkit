using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class GeoAPILineStringExtensions
    {
        public static Coordinate2D GetCoordinate2DAt(this ILineString line, int n)
        {
            var p = line.GetPointN(n);
            return new Coordinate2D(p.X, p.Y);
        }
    }
}
