using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class NtsCoordinateExtensions
    {
        public static Coordinate2D ToCoordinate2D(this GeoAPI.Geometries.Coordinate coord) =>
            new Coordinate2D(coord.X, coord.Y);
    }
}
