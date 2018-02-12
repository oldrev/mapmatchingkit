using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class Coordinate2DExtensions
    {
        public static GeoAPI.Geometries.Coordinate ToGeoAPICoordinate(this Coordinate2D self) =>
            new GeoAPI.Geometries.Coordinate(self.X, self.Y);
    }
}
