using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public static class GeoAPIMultiLineStringExtensions
    {
        public static ILineString ToLineString(this IMultiLineString mls)
        {
            var coords = mls.Geometries.Cast<ILineString>().SelectMany(g => g.Coordinates);
            return new LineString(coords.ToArray());
        }
    }
}
