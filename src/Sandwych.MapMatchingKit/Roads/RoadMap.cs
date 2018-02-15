using System;
using System.Linq;
using Sandwych.MapMatchingKit.Topology;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Spatial;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public class RoadMap : AbstractGraph<Road>
    {
        public ISpatialIndex<RoadInfo> Index { get; }
        private readonly ISpatialOperation _spatial;

        public RoadMap(IEnumerable<Road> roads, ISpatialOperation spatial) : base(roads)
        {
            _spatial = spatial;

            this.Index = new QuadtreeIndex<RoadInfo>(roads.Select(x => x.RoadInfo), spatial, r => r.Geometry);
        }

        public RoadMap(IEnumerable<Road> roads) : this(roads, GeographySpatialOperation.Instance)
        {
        }

        private IEnumerable<RoadPoint> Split(IEnumerable<(RoadInfo road, double distance)> points)
        {
            /*
             * This uses the road
             */
            foreach (var point in points)
            {
                yield return new RoadPoint(this.Edges[point.road.Id * 2], point.Item2, _spatial);

                var backwardRoadId = point.road.Id * 2 + 1;
                if (this.Edges.TryGetValue(backwardRoadId, out var road))
                {
                    yield return new RoadPoint(road, 1.0 - point.Item2, _spatial);
                }
            }
        }

        public IEnumerable<RoadPoint> Radius(Coordinate2D c, double r) =>
            this.Split(this.Index.Radius(c, r));


    }
}
