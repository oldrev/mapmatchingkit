using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Coordinate2D : ICoordinate2D, IComparable<Coordinate2D>, IEquatable<Coordinate2D>
    {
        private static readonly Coordinate2D s_nan = new Coordinate2D(double.NaN, double.NaN);

        public static ref readonly Coordinate2D NaN => ref s_nan;

        public double X { get; }

        public double Y { get; }

        public Coordinate2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Coordinate2D(in Coordinate2D c)
        {
            this.X = c.X;
            this.Y = c.Y;
        }

        public Coordinate2D(double[] coords)
        {
            if (coords.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(coords));
            }

            this.X = coords[0];
            this.Y = coords[1];
        }

        public Coordinate2D(ReadOnlySpan<double> span)
        {
            if (span.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }
            this.X = span[0];
            this.Y = span[1];
        }

        public bool IsNan => double.IsNaN(this.X) || double.IsNaN(this.Y);

        public int CompareTo(Coordinate2D other) =>
            this.CompareTo<Coordinate2D>(other);

        public int CompareTo(ICoordinate2D other) =>
            this.CompareTo<ICoordinate2D>(other);

        public bool Equals(Coordinate2D other) =>
            this.CompareTo<Coordinate2D>(other) == 0;

        public static Coordinate2D operator +(in Coordinate2D a, in Coordinate2D b) =>
            new Coordinate2D(a.X + b.X, a.Y + b.Y);

        public static Coordinate2D operator -(in Coordinate2D a, in Coordinate2D b) =>
            new Coordinate2D(a.X - b.X, a.Y - b.Y);

        public static Coordinate2D operator +(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X + f, a.Y + f);

        public static Coordinate2D operator -(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X - f, a.Y - f);

        public static Coordinate2D operator *(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X * f, a.Y * f);

        public static Coordinate2D operator /(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X / f, a.Y / f);

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public double[] ToArray() => new double[2] { this.X, this.Y };

        /// <summary>
        /// Compute Cartesian distance.
        /// </summary>
        /// <param name="other">Other coordinate to compute distance</param>
        /// <returns></returns>
        public double CartesianDistance(in Coordinate2D other)
        {
            var d1 = this.X - other.X;
            var d2 = this.Y - other.Y;
            return Math.Sqrt(d1 * d1 + d2 * d2);
        }
    }
}
