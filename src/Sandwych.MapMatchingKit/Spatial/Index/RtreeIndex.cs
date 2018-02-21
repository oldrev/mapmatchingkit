using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public class RtreeIndex<TItem> : AbstractSpatialIndex<TItem>
    {
        private readonly NetTopologySuite.Index.ISpatialIndex<TItem> _index = new STRtree<TItem>();

        protected override NetTopologySuite.Index.ISpatialIndex<TItem> Index => _index;

        public RtreeIndex(IEnumerable<TItem> items, ISpatialOperation spatialService, 
            Func<TItem, ILineString> geometryGetter, Func<TItem, double> lengthGetter)
            : base(items, spatialService, geometryGetter, lengthGetter)
        {

        }
    }
}
