using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    public class QuadtreeIndex : AbstractSpatialIndex
    {
        private readonly NetTopologySuite.Index.ISpatialIndex<SpatialIndexItem> _index = new Quadtree<SpatialIndexItem>();

        protected override NetTopologySuite.Index.ISpatialIndex<SpatialIndexItem> Index => _index;

        public QuadtreeIndex(IEnumerable<SpatialIndexItem> items) : base(items)
        {

        }

    }
}
