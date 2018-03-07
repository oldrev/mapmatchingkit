using Sandwych.MapMatchingKit.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Vector3D NaN => new Vector3D(double.NaN, double.NaN, double.NaN);
        public static Vector3D Zero => new Vector3D(0D, 0D, 0D);
        public static Vector3D One => new Vector3D(1D, 1D, 1D);

        public Vector3D(double x, double y, double z) : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3D(double[] array) : this()
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(array));
            }

            this.X = array[0];
            this.Y = array[1];
            this.Z = array[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator +(in Vector3D left, in Vector3D right) =>
            new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator -(in Vector3D left, in Vector3D right) =>
            new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator -(in Vector3D left) =>
            new Vector3D(-left.X, -left.Y, -left.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator *(in Vector3D left, double right) =>
            new Vector3D(left.X * right, left.Y * right, left.Z * right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D operator /(in Vector3D left, double right) =>
            new Vector3D(left.X / right, left.Y / right, left.Z / right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D Cross(in Vector3D other) =>
            new Vector3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Product(in Vector3D other) =>
            X * other.X + Y * other.Y + Z * other.Z;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector3D a, in Vector3D b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector3D a, in Vector3D b) => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3D other) =>
            this.X == other.X && this.Y == other.Y && this.Z == other.Z;


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
                    case 2: return Z;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCodeHelper.Combine(X.GetHashCode(), Y.GetHashCode(), Z.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double[] ToArray() => new double[3] { X, Y, Z };

        public override string ToString() =>
            string.Format("Vector3D({0}, {1}, {2})", X, Y, Z);
    }
}
