using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;

namespace Sandwych.MapMatchingKit.Spatial
{
    public abstract class AbstractSpatialIndex : ISpatialIndex
    {
        protected abstract NetTopologySuite.Index.ISpatialIndex<SpatialIndexItem> Index { get; }

        protected AbstractSpatialIndex()
        {

        }

        public List<(int id, double distance)> Radius(Point c, double radius)
        {
            var neighbors = new List<(int id, double distance)>();
            var env = c.EnvelopeInternal;

            var visitor = new IndexItemVisitor<SpatialIndexItem>(item =>
            {
                /*
                double f = spatial.intercept(geometry, c);
                Point p = spatial.interpolate(geometry, spatial.length(geometry), f);
                double d = spatial.distance(p, c);

                if (d < radius)
                {
                    neighbors.Add((nid, f));
                }
                */
            });

            Index.Query(env, visitor);

            return neighbors;
        }

        protected void Add(SpatialIndexItem item)
        {
            var env = item.Geometry.EnvelopeInternal;
            Index.Insert(env, item);
        }
    }
}
