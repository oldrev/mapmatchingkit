using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Index;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public abstract class AbstractSpatialIndex<TItem> : ISpatialIndex<TItem>
    {
        protected abstract NetTopologySuite.Index.ISpatialIndex<TItem> Index { get; }
        protected ISpatialOperation Spatial { get; }
        protected Func<TItem, ILineString> ItemGeometryGetter { get; }
        protected Func<TItem, double> ItemLengthGetter { get; }

        public AbstractSpatialIndex(IEnumerable<TItem> items, ISpatialOperation spatialService, 
            Func<TItem, ILineString> geometryGetter, Func<TItem, double> lengthGetter)
        {
            this.ItemGeometryGetter = geometryGetter;
            this.ItemLengthGetter = lengthGetter;
            this.Spatial = spatialService;
            this.AddRange(items);
        }

        public IReadOnlyList<(TItem, double)> Radius(Coordinate2D c, double radius)
        {
            var neighbors = new List<(TItem, double)>();
            var env = this.Spatial.Envelope(c, radius);

            var visitor = new IndexItemVisitor<TItem>(item =>
            {
                var geometry = this.ItemGeometryGetter(item);
                var f = this.Spatial.Intercept(geometry, c);
                var p = this.Spatial.Interpolate(geometry, this.ItemLengthGetter(item), f);
                var d = this.Spatial.Distance(p, c);

                if (d < radius)
                {
                    neighbors.Add((item, f));
                }
            });

            Index.Query(env, visitor);

            return neighbors;
        }

        protected void Add(TItem item)
        {
            var geom = this.ItemGeometryGetter(item);
            var env = this.Spatial.Envelope(geom as ILineString);
            this.Index.Insert(env, item);
        }

        protected void AddRange(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

    }
}
