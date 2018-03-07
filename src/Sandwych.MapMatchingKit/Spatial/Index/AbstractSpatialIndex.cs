using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public abstract class AbstractSpatialIndex<TItem> : ISpatialIndex<TItem>
    {
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

        public virtual IEnumerable<(TItem Item, double Distance)> Radius(in Coordinate2D c, double radius, int k = -1)
        {
            var neighbors = new List<(TItem Item, double Distance)>(20);
            var env = this.Spatial.Envelope(c, radius);
            var candidates = this.Search(env);
            foreach (var candidate in candidates)
            {
                var geometry = this.ItemGeometryGetter(candidate);
                var f = this.Spatial.Intercept(geometry, c);
                var p = this.Spatial.Interpolate(geometry, this.ItemLengthGetter(candidate), f);
                var d = this.Spatial.Distance(p, c);

                if (d <= radius)
                {
                    neighbors.Add((candidate, f));
                }
            }

            if (k > 0)
            {
                return neighbors.OrderBy(i => i.Distance).Take(k);
            }
            else
            {
                return neighbors;
            }
        }

        protected abstract void Add(TItem item);
        protected abstract void AddRange(IEnumerable<TItem> items);

        public abstract IEnumerable<TItem> Search(Envelope envelope);

    }

}
