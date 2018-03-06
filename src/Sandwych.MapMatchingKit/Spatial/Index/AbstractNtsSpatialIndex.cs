using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Index;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public abstract class AbstractNtsSpatialIndex<TItem> : AbstractSpatialIndex<TItem>
    {
        protected abstract NetTopologySuite.Index.ISpatialIndex<TItem> Index { get; }

        protected AbstractNtsSpatialIndex(IEnumerable<TItem> items, ISpatialOperation spatialService,
                Func<TItem, ILineString> geometryGetter, Func<TItem, double> lengthGetter)
                : base(items, spatialService, geometryGetter, lengthGetter)
        {

        }

        protected override void Add(TItem item)
        {
            var geom = this.ItemGeometryGetter(item);
            var env = this.Spatial.Envelope(geom as ILineString);
            var ntsEnv = new GeoAPI.Geometries.Envelope(env.MinX, env.MaxX, env.MinY, env.MaxY);
            this.Index.Insert(ntsEnv, item);
        }

        protected override void AddRange(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        public override IEnumerable<TItem> Search(Envelope envelope)
        {
            return this.Index.Query(envelope);
        }

    }
}
