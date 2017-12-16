using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{

    public class StrtreeIndex : AbstractSpatialIndex
    {
        private readonly NetTopologySuite.Index.ISpatialIndex<SpatialIndexItem> _index = new STRtree<SpatialIndexItem>();

        protected override NetTopologySuite.Index.ISpatialIndex<SpatialIndexItem> Index => _index;
    }

}
