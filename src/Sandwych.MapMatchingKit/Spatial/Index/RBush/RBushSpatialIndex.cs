using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Index;

namespace Sandwych.MapMatchingKit.Spatial.Index.RBush
{
    public class RBushSpatialIndex<TItem> : AbstractSpatialIndex<TItem>
    {
        private sealed class NtsSpatialIndex : NetTopologySuite.Index.ISpatialIndex<TItem>
        {
            private readonly global::RBush.RBush<RBushIndexItem> _backend = new global::RBush.RBush<RBushIndexItem>();

            public void Insert(Envelope itemEnv, TItem item)
            {
                var env = new global::RBush.Envelope(itemEnv.MinX, itemEnv.MinY, itemEnv.MaxX, itemEnv.MaxY);
                _backend.Insert(new RBushIndexItem(item, env));
            }

            public IList<TItem> Query(Envelope searchEnv)
            {
                throw new NotSupportedException();
            }

            public void Query(Envelope searchEnv, IItemVisitor<TItem> visitor)
            {
                var env = new global::RBush.Envelope(searchEnv.MinX, searchEnv.MinY, searchEnv.MaxX, searchEnv.MaxY);
                var items = _backend.Search(env);
                foreach (var i in items)
                {
                    visitor.VisitItem(i.Item);
                }
            }

            public bool Remove(Envelope itemEnv, TItem item)
            {
                throw new NotSupportedException();
            }
        }

        private class RBushIndexItem : global::RBush.ISpatialData
        {
            private global::RBush.Envelope _envelope;

            public RBushIndexItem(TItem item, in global::RBush.Envelope envelope)
            {
                this.Item = item;
                this._envelope = envelope;
            }

            public TItem Item { get; }

            public ref readonly global::RBush.Envelope Envelope => ref _envelope;
        }

        private readonly NetTopologySuite.Index.ISpatialIndex<TItem> _index = new NtsSpatialIndex();
        protected override NetTopologySuite.Index.ISpatialIndex<TItem> Index => _index;

        public RBushSpatialIndex(
            IEnumerable<TItem> items, ISpatialOperation spatialService,
            Func<TItem, ILineString> geometryGetter, Func<TItem, double> lengthGetter)
            : base(items, spatialService, geometryGetter, lengthGetter)
        {
        }

    }
}
