using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Index;

namespace Sandwych.MapMatchingKit.Spatial.Index.RBush
{
    using Sandwych.MapMatchingKit.Spatial.Geometries;
    using System.Linq;
    using RB = global::RBush;

    public class RBushSpatialIndex<TItem> : AbstractSpatialIndex<TItem>
    {
        private class RBushIndexItem : RB.ISpatialData
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

        private readonly RB.RBush<RBushIndexItem> _index = new RB.RBush<RBushIndexItem>();

        public RBushSpatialIndex(
            IEnumerable<TItem> items, ISpatialOperation spatialService,
            Func<TItem, ILineString> geometryGetter, Func<TItem, double> lengthGetter)
            : base(items, spatialService, geometryGetter, lengthGetter)
        {
        }

        protected override void Add(TItem item)
        {
            var env = this.Spatial.Envelope(ItemGeometryGetter(item) as ILineString);
            var rbEnv = new RB.Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);
            var rbItem = new RBushIndexItem(item, rbEnv);
            _index.Insert(rbItem);
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
            var rbEnv = new RB.Envelope(envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
            return _index.Search(rbEnv).Select(e => e.Item);
        }
    }
}
