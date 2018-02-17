using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public interface ISpatialIndex<TItem>
    {
        IReadOnlyList<(TItem, double)> Radius(Coordinate2D c, double radius);
    }
}
