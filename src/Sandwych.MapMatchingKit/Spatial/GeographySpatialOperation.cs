using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using GeographicLib;

namespace Sandwych.MapMatchingKit.Spatial
{
    public sealed class GeographySpatialOperation : ISpatialOperation
    {
        private static readonly Lazy<ISpatialOperation> s_instance = new Lazy<ISpatialOperation>(() => new GeographySpatialOperation(), true);

        public static ISpatialOperation Instance => s_instance.Value;

        public double Azimuth(in Coordinate2D a, in Coordinate2D b, double f)
        {
            double azi = 0;
            if (f < 0 + 1E-10)
            {
                azi = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X).azi1;
            }
            else if (f > 1 - 1E-10)
            {
                azi = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X).azi2;
            }
            else
            {
                var c = this.Interpolate(a, b, f);
                azi = Geodesic.WGS84.Inverse(a.Y, a.X, c.Y, c.X).azi2;
            }
            return azi < 0 ? azi + 360 : azi;
        }

        public double Azimuth(ILineString path, double f) =>
            this.Azimuth(path, this.Length(path), f);

        public double Azimuth(ILineString path, double l, double f)
        {
            if (!(f >= 0 && f <= 1))
            {
                throw new ArgumentOutOfRangeException(nameof(f));
            }

            var a = path.GetPointN(0).ToCoordinate2D();
            double d = l * f;
            double s = 0, ds = 0;

            if (f < 0 + 1E-10)
            {
                return this.Azimuth(path.GetPointN(0).ToCoordinate2D(), path.GetPointN(1).ToCoordinate2D(), 0);
            }

            if (f > 1 - 1E-10)
            {
                return this.Azimuth(path.GetPointN(path.NumPoints - 2).ToCoordinate2D(),
                    path.GetPointN(path.NumPoints - 1).ToCoordinate2D(), f);
            }

            for (int i = 1; i < path.NumPoints; ++i)
            {
                var b = path.GetPointN(i).ToCoordinate2D();
                ds = this.Distance(a, b);

                if ((s + ds) >= d)
                {
                    return this.Azimuth(a, b, (d - s) / ds);
                }

                s = s + ds;
                a = b;
            }

            return double.NaN;
        }

        public double Distance(in Coordinate2D a, in Coordinate2D b) =>
            Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X).s12;

        public Envelope Envelope(in Coordinate2D c, double radius)
        {
            var ymax = Geodesic.WGS84.Direct(c.Y, c.X, 0, radius).lat2;
            var ymin = Geodesic.WGS84.Direct(c.Y, c.X, -180, radius).lat2;
            var xmax = Geodesic.WGS84.Direct(c.Y, c.X, 90, radius).lon2;
            var xmin = Geodesic.WGS84.Direct(c.Y, c.X, -90, radius).lon2;
            return new Envelope(xmin, ymin, xmax, ymax);
        }

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

        public double Intercept(in Coordinate2D a, in Coordinate2D b, in Coordinate2D c)
        {
            if (a.X == b.X && a.Y == b.Y)
            {
                return 0;
            }
            var inter = new GeodesicInterception(Geodesic.WGS84);
            var ci = inter.Intercept(a.Y, a.X, b.Y, b.X, c.Y, c.X);
            var ai = Geodesic.WGS84.Inverse(a.Y, a.X, ci.lat2, ci.lon2);
            var ab = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X);
            return (Math.Abs(ai.azi1 - ab.azi1) < 1) ? ai.s12 / ab.s12 : (-1) * ai.s12 / ab.s12;
        }

        public Coordinate2D Interpolate(in Coordinate2D a, in Coordinate2D b, double f)
        {
            var inv = Geodesic.WGS84.Inverse(a.Y, a.X, b.Y, b.X);
            var pos = Geodesic.WGS84.Line(inv.lat1, inv.lon1, inv.azi1).Position(inv.s12 * f);
            return new Coordinate2D(pos.lon2, pos.lat2);
        }

        public Coordinate2D Interpolate(ILineString path, double f) =>
            this.Interpolate(path, this.Length(path), f);

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

        public double Length(ILineString line)
        {
            var d = 0D;
            for (var i = 1; i < line.NumPoints; ++i)
            {
                d += this.Distance(line.GetPointN(i - 1).ToCoordinate2D(), line.GetPointN(i).ToCoordinate2D());
            }
            return d;
        }

        public double Length(IMultiLineString line) =>
            line.Geometries.Cast<ILineString>().Sum(x => this.Length(x));

    }
}
