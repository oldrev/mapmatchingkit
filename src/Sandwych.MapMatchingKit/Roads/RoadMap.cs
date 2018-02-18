using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Sandwych.MapMatchingKit.Topology;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Spatial.Index;

namespace Sandwych.MapMatchingKit.Roads
{
    /// Implementation of a road map with (directed) roads, i.e. {@link Road} objects. It provides a road
    /// network for routing that is derived from {@link Graph} and spatial search of roads with a
    /// {@link SpatialIndex}.
    /// <para>
    /// <b>Note:</b> Since {@link Road} objects are directed representations of {@link BaseRoad} objects,
    /// identifiers have a special mapping, see {@link Road}.
    /// </para>
    public sealed class RoadMap : AbstractGraph<Road>
    {
        public ISpatialIndex<RoadInfo> Index { get; }
        private readonly ISpatialOperation _spatial;

        public RoadMap(IEnumerable<Road> roads, ISpatialOperation spatial) : base(roads)
        {
            _spatial = spatial;
            this.Index = new RtreeIndex<RoadInfo>(roads.Select(x => x.RoadInfo), spatial, r => r.Geometry, r => r.Length);
        }

        public RoadMap(IEnumerable<Road> roads) : this(roads, GeographySpatialOperation.Instance)
        {
        }

        private IEnumerable<RoadPoint> Split(IEnumerable<(RoadInfo road, double fraction)> points)
        {
            /*
             * This uses the road
             */
            foreach (var point in points)
            {
                yield return new RoadPoint(this.Edges[point.road.Id * 2], point.fraction, _spatial);

                var backwardRoadId = point.road.Id * 2 + 1;
                if (this.Edges.TryGetValue(backwardRoadId, out var road))
                {
                    yield return new RoadPoint(road, 1.0 - point.fraction, _spatial);
                }
            }
        }

        public IEnumerable<RoadPoint> Radius(in Coordinate2D c, double r) =>
            this.Split(this.Index.Radius(c, r));

    }
}
