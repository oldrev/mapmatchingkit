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
        public ISpatialIndex<Road> Index { get; }
        private readonly ISpatialOperation _spatial;

        public RoadMap(IEnumerable<Road> roads, ISpatialOperation spatial) : base(roads)
        {
            _spatial = spatial;
            this.Index = new QuadtreeIndex<Road>(roads, spatial, r => r.Geometry);
        }

        public RoadMap(IEnumerable<Road> roads) : this(roads, CartesianSpatialOperation.Instance)
        {
        }

        private IEnumerable<RoadPoint> Split(IEnumerable<(Road road, double distance)> points)
        {
            /*
             * This uses the road
             */
            foreach (var point in points)
            {
                yield return RoadPoint.FromRoadFraction(this.Edges[point.road.Id * 2], point.Item2, _spatial);

                if (this.Edges.ContainsKey(point.road.Id * 2 + 1))
                {
                    yield return RoadPoint.FromRoadFraction(this.Edges[point.road.Id * 2 + 1], 1.0 - point.Item2, _spatial);
                }
            }
        }

        public IEnumerable<RoadPoint> Radius(Coordinate2D c, double r) =>
            this.Split(this.Index.Radius(c, r));


    }
}
