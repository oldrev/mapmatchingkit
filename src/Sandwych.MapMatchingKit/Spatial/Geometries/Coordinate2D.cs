using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GeoAPI.Geometries;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Coordinate2D : ICoordinate2D, IComparable<Coordinate2D>, IEquatable<Coordinate2D>
    {
        public static Coordinate2D NaN => new Coordinate2D(double.NaN, double.NaN);

        public static Coordinate2D Origin => new Coordinate2D(0D, 0D);

        public static Coordinate2D Zero => Origin;

        public static Coordinate2D One => new Coordinate2D(1D, 1D);

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
            if (coords == null)
            {
                throw new ArgumentNullException(nameof(coords));
            }

            if (coords.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(coords));
            }

            this.X = coords[0];
            this.Y = coords[1];
        }

        public bool IsNan => double.IsNaN(this.X) || double.IsNaN(this.Y);

        public double this[Ordinate index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Coordinate2D other)
        {
            if (X < other.X)
            {
                return -1;
            }
            if (X > other.X)
            {
                return 1;
            }
            if (Y < other.Y)
            {
                return -1;
            }
            return Y > other.Y ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ICoordinate2D other)
        {
            if (X < other.X)
            {
                return -1;
            }
            if (X > other.X)
            {
                return 1;
            }
            if (Y < other.Y)
            {
                return -1;
            }
            return Y > other.Y ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator +(in Coordinate2D a, in Coordinate2D b) =>
            new Coordinate2D(a.X + b.X, a.Y + b.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator -(in Coordinate2D a, in Coordinate2D b) =>
            new Coordinate2D(a.X - b.X, a.Y - b.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator -(in Coordinate2D a) =>
            new Coordinate2D(-a.X, -a.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator +(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X + f, a.Y + f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator -(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X - f, a.Y - f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator *(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X * f, a.Y * f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate2D operator /(in Coordinate2D a, double f) =>
            new Coordinate2D(a.X / f, a.Y / f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Coordinate2D a, in Coordinate2D b) =>
            a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Coordinate2D a, in Coordinate2D b) =>
            !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Coordinate2D other) =>
            this.CompareTo<Coordinate2D>(other) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Coordinate2D p && this.Equals(p);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() =>
            (X, Y).GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] ToArray() => new double[2] { this.X, this.Y };

        /// <summary>
        /// Compute Cartesian distance.
        /// </summary>
        /// <param name="other">Other coordinate to compute distance</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CartesianDistance(in Coordinate2D other)
        {
            var d1 = this.X - other.X;
            var d2 = this.Y - other.Y;
            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        public override string ToString() =>
            string.Format("Coordinate2D({0}, {1})", X, Y);
    }
}
