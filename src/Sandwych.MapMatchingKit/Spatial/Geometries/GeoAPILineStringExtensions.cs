using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class GeoAPILineStringExtensions
    {
        public static Coordinate2D GetCoordinate2DAt(this ILineString line, int n) =>
            line.GetCoordinateN(n).ToCoordinate2D();
    }
}
