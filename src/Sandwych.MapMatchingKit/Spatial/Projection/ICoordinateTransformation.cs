using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Projection
{
    public interface ICoordinateTransformation
    {
        double[] Transform(double[] from);
        Coordinate2D Transform(Coordinate2D from);
        Coordinate2D[] BulkTransform(IEnumerable<Coordinate2D> from);
    }

    public static class CoordinateTransformationExtensions
    {
        public static ILineString Transform(this ICoordinateTransformation self, ILineString line)
        {
            var newCoords = new Coordinate[line.NumPoints];
            for (var i = 0; i < line.NumPoints; i++)
            {
                var pt = line.GetCoordinateN(i);
                var coord = newCoords[i];
                var oldCoord = new Coordinate2D(pt.X, pt.Y);
                var transformedCoord = self.Transform(oldCoord);
                newCoords[i] = new Coordinate(transformedCoord.X, transformedCoord.Y);
            }
            return new LineString(newCoords);
        }


    }
}
