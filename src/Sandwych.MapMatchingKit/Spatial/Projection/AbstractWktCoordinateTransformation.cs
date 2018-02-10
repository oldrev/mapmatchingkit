using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using GeoAPI.CoordinateSystems;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using ProjNet.Converters.WellKnownText;

namespace Sandwych.MapMatchingKit.Spatial.Projection
{
    public abstract class AbstractWktCoordinateTransformation : ICoordinateTransformation
    {
        protected string FromWkt { get; }
        protected string ToWkt { get; }
        protected ICoordinateSystem FromCrs { get; }
        protected ICoordinateSystem ToCrs { get; }
        protected GeoAPI.CoordinateSystems.Transformations.ICoordinateTransformation InternalCoordinateTransformation { get; }

        public AbstractWktCoordinateTransformation(string fromWkt, string toWkt)
        {
            this.FromWkt = fromWkt;
            this.ToWkt = toWkt;
            this.FromCrs = CoordinateSystemWktReader.Parse(fromWkt, Encoding.ASCII) as ICoordinateSystem;
            this.ToCrs = CoordinateSystemWktReader.Parse(toWkt, Encoding.ASCII) as ICoordinateSystem;
            var factory = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            this.InternalCoordinateTransformation = factory.CreateFromCoordinateSystems(this.FromCrs, this.ToCrs);
        }

        public double[] Transform(double[] from) =>
            this.InternalCoordinateTransformation.MathTransform.Transform(from);

        public Coordinate2D Transform(Coordinate2D from)
        {
            var outArray = this.Transform(from.ToArray());
            return new Coordinate2D(outArray[0], outArray[1]);
        }

        public Coordinate2D[] BulkTransform(IEnumerable<Coordinate2D> from)
        {
            var inArray = new double[2];
            var output = new Coordinate2D[from.Count()];
            int i = 0;
            foreach (var coord in from)
            {
                inArray[0] = coord.X;
                inArray[1] = coord.Y;
                var outCoord = this.Transform(inArray);
                output[i] = new Coordinate2D(outCoord[0], outCoord[1]);
                i++;
            }
            return output;
        }
    }
}
