using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public class QuadtreeIndex<TItem> : AbstractSpatialIndex<TItem>
    {
        private readonly NetTopologySuite.Index.ISpatialIndex<TItem> _index = new Quadtree<TItem>();

        protected override NetTopologySuite.Index.ISpatialIndex<TItem> Index => _index;

        public QuadtreeIndex(IEnumerable<TItem> items, ISpatialOperation spatial, Func<TItem, IGeometry> geomGetter) : base(items, spatial, geomGetter)
        {

        }

    }
}
