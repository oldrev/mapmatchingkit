using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class Coordinate2DExtensions
    {
        //Add workaround for C# 7.2's bug
        //(this in Coordinate2D self)

        public static GeoAPI.Geometries.Coordinate ToGeoAPICoordinate(in Coordinate2D self) =>
            new GeoAPI.Geometries.Coordinate(self.X, self.Y);

        public static GeoAPI.Geometries.IPoint ToGeoAPIPoint(in Coordinate2D self) =>
            new Point(self.X, self.Y);
    }
}
