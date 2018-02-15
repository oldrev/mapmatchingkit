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

#if CARTESIAN_SPATIAL
    public sealed class CartesianSpatialOperation : ISpatialOperation
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
            var a = p.GetPointN(0).ToCoordinate2D();
            var s = 0D;
            var sf = 0D;
            var ds = 0D;

            for (int i = 1; i < p.NumPoints; ++i)
            {
                var b = p.GetPointN(i).ToCoordinate2D();

                ds = this.Distance(a, b);

                var f_ = this.Intercept(a, b, c);
                f_ = (f_ > 1) ? 1 : (f_ < 0) ? 0 : f_;
                var x = this.Interpolate(a, b, f_);
                double d_ = this.Distance(c, x);

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
            var d_ab = a.CartesianDistance(b);
            var d_ap = a.CartesianDistance(p);
            var d_abp = GeometryMath.DistancePointLinePerpendicular(p, a, b);
            var d = Math.Sqrt(d_ap * d_ap - d_abp * d_abp);
            return d / d_ab;
        }

        public double Azimuth(in Coordinate2D a, in Coordinate2D b, double f)
        {
            var d = b - a;
            return 90d - (180d / Math.PI * Math.Atan2(d.Y, d.X));
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
            var p0 = path.GetPointN(0).ToCoordinate2D();

            var a = p0;
            double d = l * f;
            double s = 0, ds = 0;

            if (f < 0 + 1E-10)
            {
                return p0;
            }

            if (f > 1 - 1E-10)
            {
                return path.GetPointN(path.NumPoints - 1).ToCoordinate2D();
            }

            for (int i = 1; i < path.NumPoints; ++i)
            {
                var b = path.GetPointN(i).ToCoordinate2D();
                ds = this.Distance(a, b);

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
            return a + d;
        }


        public Envelope Envelope(in Coordinate2D c, double radius)
        {
            //c 是圆心，radius 是外接正方形边长的一半
            var bottomLeft = (x: c.X - radius, y: c.Y - radius);
            var topRight = (x: c.X + radius, y: c.Y + radius);
            return new Envelope(bottomLeft.x, topRight.x, bottomLeft.y, topRight.y);
        }



    }

#endif
}
