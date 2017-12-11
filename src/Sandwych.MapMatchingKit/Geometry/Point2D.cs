using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Geometry
{
    public readonly struct Point2D : IEquatable<Point2D>
    {
        private readonly double _x;
        private readonly double _y;

        public Point2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public double X => _x;
        public double Y => _y;

        public bool Equals(Point2D other)
        {
            return _x == other.X && _y == other.Y;
        }

        public bool Equals(double otherX, double otherY)
        {
            return _x == otherX && _y == otherY;
        }

        public bool Equals(Point2D other, double tol)
        {
            return (Math.Abs(_x - other.X) <= tol) && (Math.Abs(_y - other.Y) <= tol);
        }

    }
}
