using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial
{
    public interface ISpatialIndex
    {
        List<(int id, double distance)> Radius(Point c, double radius);
    }
}
