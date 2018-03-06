using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandwych.MapMatchingKit.Spatial;

namespace Sandwych.MapMatchingKit.Roads
{

    public class RoadMapBuilder : IRoadMapBuilder
    {
        private readonly IDictionary<long, RoadInfo> _roads = new Dictionary<long, RoadInfo>();
        private readonly ISpatialOperation _spatial;

        public RoadMapBuilder(ISpatialOperation spatial)
        {
            _spatial = spatial;
        }


        public IRoadMapBuilder AddRoad(RoadInfo road)
        {
            _roads.Add(road.Id, road);
            return this;
        }

        public IRoadMapBuilder AddRoads(IEnumerable<RoadInfo> roads)
        {
            foreach (var r in roads)
            {
                this.AddRoad(r);
            }
            return this;
        }

        public RoadMap Build()
        {
            return new RoadMap(this.GetAllRoads(), _spatial);
        }

        private IEnumerable<Road> GetAllRoads()
        {
            foreach (var roadInfo in _roads.Values)
            {
                if (roadInfo.OneWay)
                {
                    yield return new Road(roadInfo, Heading.Forward);
                }
                else
                {
                    yield return new Road(roadInfo, Heading.Forward);
                    yield return new Road(roadInfo, Heading.Backward);
                }
            }
        }

        private static RoadMap ConstructEdges(RoadMap graph)
        {
            var map = new Dictionary<long, IList<Road>>();

            foreach (var edge in graph.EdgeMap.Values)
            {
                if (!map.ContainsKey(edge.Source))
                {
                    map[edge.Source] = new List<Road>() { edge };
                }
                else
                {
                    map[edge.Source].Add(edge);
                }
            }

            IList<Road> successors = null;
            foreach (var edges in map.Values)
            {
                for (int i = 1; i < edges.Count; ++i)
                {
                    var prevEdge = edges[i - 1];
                    prevEdge.Neighbor = edges[i];

                    prevEdge.Successor = map.TryGetValue(prevEdge.Target, out successors) ? successors.First() : default;
                }

                var lastEdge = edges.Last();
                lastEdge.Neighbor = edges.First();
                lastEdge.Successor = map.TryGetValue(lastEdge.Target, out successors) ? successors.First() : default;
            }
            return graph;
        }


    }
}
