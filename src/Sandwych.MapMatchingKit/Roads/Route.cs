using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandwych.MapMatchingKit.Topology;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{
    public sealed class Route : IPath<Road, RoadPoint>, IEquatable<Route>
    {
        private static readonly IEnumerable<Road> EmptyEdges = new Road[] { };
        private readonly IEnumerable<Road> _edges;
        private readonly RoadPoint _startPoint;
        private readonly RoadPoint _endPoint;

        public ref readonly RoadPoint StartPoint => ref _startPoint;

        public ref readonly RoadPoint EndPoint => ref _endPoint;

        public IEnumerable<Road> Edges => GetEdges(this.StartPoint, this.EndPoint, this._edges);

        public float Length { get; }

        public Route(in RoadPoint startPoint, in RoadPoint endPoint, IEnumerable<Road> edges)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _edges = edges;
            this.Length = ComputeLength(startPoint, endPoint, edges);
        }

        private static float ComputeLength(in RoadPoint startPoint, in RoadPoint endPoint, in IEnumerable<Road> edges)
        {
            var edges_ = GetEdges(startPoint, endPoint, edges);
            var totalLength = edges_.Sum(r => r.Length);
            var d = totalLength - (startPoint.Fraction * startPoint.Edge.Length) - ((1.0 - endPoint.Fraction) * endPoint.Edge.Length);
            return (float)d;
        }

        private static IEnumerable<Road> GetEdges(RoadPoint startPoint, RoadPoint endPoint, IEnumerable<Road> edges)
        {
            yield return startPoint.Edge;
            foreach (var edge in edges)
            {
                yield return edge;
            }
            yield return endPoint.Edge;
        }

        public double Cost(Func<Road, double> costFunc)
        {
            var value = Costs.Cost(this.StartPoint.Edge, 1.0 - this.StartPoint.Fraction, costFunc);

            foreach (var e in _edges)
            {
                value += costFunc(e);
            }

            value -= Costs.Cost(this.EndPoint.Edge, 1.0 - this.EndPoint.Fraction, costFunc);

            return value;
        }

        public ILineString ToGeometry()
        {
            var coords = this.Edges.Select(e => e.Geometry).SelectMany(e => e.Coordinates).ToArray();
            var geom = new LineString(coords);
            return geom;
        }

        public bool Equals(Route other)
        {
            if (!this.StartPoint.Equals(other.StartPoint))
            {
                return false;
            }

            if (!this.EndPoint.Equals(other.EndPoint))
            {
                return false;
            }

            return this.Edges.SequenceEqual(other.Edges);
        }
    }

}
