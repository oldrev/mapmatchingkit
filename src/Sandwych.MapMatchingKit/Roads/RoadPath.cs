using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Topology;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public readonly struct RoadPath : IPath<Road, RoadPoint>
    {
        private readonly IEnumerable<Road> _edges;

        public RoadPoint StartPoint { get; }

        public RoadPoint EndPoint { get; }

        public IEnumerable<Road> Edges => GetEdges(this.StartPoint, this.EndPoint, this._edges);

        public double Distance { get; }

        public RoadPath(RoadPoint startPoint, RoadPoint endPoint, IEnumerable<Road> edges)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this._edges = edges;
            this.Distance = ComputeDistance(startPoint, endPoint, edges);
        }

        private static double ComputeDistance(in RoadPoint startPoint, in RoadPoint endPoint, in IEnumerable<Road> edges = null)
        {
            var edges_ = GetEdges(startPoint, endPoint, edges);
            var totalLength = edges_.Sum(r => r.Length);
            return totalLength - (startPoint.Fraction * startPoint.Edge.Length) - ((1.0 - endPoint.Fraction) * endPoint.Edge.Length);
        }

        private static IEnumerable<Road> GetEdges(RoadPoint startPoint, RoadPoint endPoint, IEnumerable<Road> edges)
        {
            if (startPoint.Edge.Target == endPoint.Edge.Source)
            {
                yield return startPoint.Edge;
                yield return endPoint.Edge;
            }
            else
            {
                foreach (var edge in edges)
                {
                    yield return edge;
                }
            }
        }

        public MultiLineString ToGeometry()
        {
            var lineStrings = this.Edges.SelectMany(e => e.Geometry).Cast<ILineString>().ToArray();
            var geom = new MultiLineString(lineStrings);
            return geom;
        }

    }

}
