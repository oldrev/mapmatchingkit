using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial
{
    public readonly struct Vector3D
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Vector3D NaN => new Vector3D(double.NaN, double.NaN, double.NaN);

        public Vector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
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

    }
}
