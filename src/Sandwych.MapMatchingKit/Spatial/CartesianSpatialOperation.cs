using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using System.Numerics;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    public class CartesianSpatialOperation : ISpatialOperation
    {
        private const double TwoPi = Math.PI * 2;
        private const double Rad2Deg = 180.0 / Math.PI;

        private static readonly Lazy<ISpatialOperation> s_instance = new Lazy<ISpatialOperation>(() => new CartesianSpatialOperation(), true);

        public static ISpatialOperation Instance => s_instance.Value;

        public double Distance(in Coordinate2D a, in Coordinate2D b) =>
            a.CartesianDistance(b);

        public double Length(ILineString path) => path.Length;

        public double Length(IMultiLineString path) => path.Length;

        public double Intercept(ILineString p, in Coordinate2D c)
        {
            var d = Double.MaxValue;
            var a = p.GetCoordinate2DAt(0);
            double s = 0, sf = 0, ds = 0;

            for (int i = 1; i < p.NumPoints; ++i)
            {
                var b = p.GetCoordinate2DAt(i);
                ds = a.CartesianDistance(b);
                var f_ = this.Intercept(a, b, c);
                f_ = (f_ > 1) ? 1 : (f_ < 0) ? 0 : f_;
                var x = this.Interpolate(a, b, f_);
                var d_ = c.CartesianDistance(x);

                if (d_ < d)
                {
                    sf = (f_ * ds) + s;
                    d = d_;
                }

                s = s + ds;
                a = b;
            }

            return s == 0 ? 0 : sf / s;
        }

        public double Intercept(in Coordinate2D a, in Coordinate2D b, in Coordinate2D p)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*(2)
                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = -----------------------------
                                         Curve^2
                        Then the distance from C to Point = |s|*Curve.
            */
            var len2 = ((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
            var s = ((a.Y - p.Y) * (b.X - a.X) - (a.X - p.X) * (b.Y - a.Y)) / len2;
            return Math.Abs(s) * Math.Sqrt(len2);
        }

        public double Azimuth(in Coordinate2D a, in Coordinate2D b, double f)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            return 90.0 - Rad2Deg * Math.Atan2(dy, dx);
        }

        public double Azimuth(ILineString path, double f)
        {
            var l = this.Length(path);
            return this.Azimuth(path, l, f);
        }

        public double Azimuth(ILineString path, double l, double f)
        {
            var d = l * f;
            var s = 0.0;
            for (int i = 1; i < path.NumPoints; i++)
            {
                var a = path.GetCoordinate2DAt(i - 1);
                var b = path.GetCoordinate2DAt(i);
                var ds = a.CartesianDistance(b);
                if ((s + ds) >= d)
                {
                    return this.Azimuth(a, b, (d - s) / ds);
                }
                s += ds;
            }
            return double.NaN;
        }

        public Coordinate2D Interpolate(ILineString path, double f)
        {
            var l = this.Length(path);
            return this.Interpolate(path, l, f);
        }

        public Coordinate2D Interpolate(ILineString path, double l, double f)
        {
            if (!(f >= 0 && f <= 1))
            {
                throw new ArgumentOutOfRangeException(nameof(f));
            }

            var a = path.GetCoordinate2DAt(0);
            double d = l * f;
            double s = 0, ds = 0;

            if (f < 0 + 1E-10)
            {
                return a;
            }

            if (f > 1 - 1E-10)
            {
                return path.GetCoordinate2DAt(path.NumPoints - 1);
            }

            for (int i = 1; i < path.NumPoints; ++i)
            {
                var b = path.GetCoordinate2DAt(i);
                ds = a.CartesianDistance(b);

                if ((s + ds) >= d)
                {
                    return this.Interpolate(a, b, (d - s) / ds);
                }

                s = s + ds;
                a = b;
            }

            return Coordinate2D.NaN;
        }

        public Coordinate2D Interpolate(in Coordinate2D a, in Coordinate2D b, double f)
        {
            var l = a.CartesianDistance(b);
            var d = l * f;
            return new Coordinate2D(a.X + d, a.Y + d);
        }


        public Envelope Envelope(in Coordinate2D c, double radius)
        {
            //c 是圆心，radius 是外接正方形边长的一半
            var bottomLeft = (x: c.X - radius, y: c.Y - radius);
            var topRight = (x: c.X + radius, y: c.Y + radius);
            return new Envelope(bottomLeft.x, topRight.x, bottomLeft.y, topRight.y);
        }

    }
}
