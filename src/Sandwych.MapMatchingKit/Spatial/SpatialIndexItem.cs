using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    public readonly struct SpatialIndexItem
    {
        public long Id { get; }
        public IGeometry Geometry { get; }

        public SpatialIndexItem(long id, IGeometry geom)
        {
            this.Id = id;
            this.Geometry = geom;
        }
    }
}
