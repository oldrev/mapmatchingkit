using System;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Projection
{
    public interface ICoordinateTransformation
    {
        double[] Transform(double[] from);
        Coordinate2D Transform(Coordinate2D from);
        Coordinate2D[] BulkTransform(IEnumerable<Coordinate2D> from);
    }
}
