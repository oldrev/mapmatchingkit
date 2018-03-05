using System;

namespace RBush
{
    public readonly struct Envelope
    {
        public double MinX { get; }
        public double MinY { get; }
        public double MaxX { get; }
        public double MaxY { get; }

        public double Area => Math.Max(this.MaxX - this.MinX, 0) * Math.Max(this.MaxY - this.MinY, 0);
        public double Margin => Math.Max(this.MaxX - this.MinX, 0) + Math.Max(this.MaxY - this.MinY, 0);

        public Envelope(double minX, double minY, double maxX, double maxY)
        {
            this.MinX = minX;
            this.MinY = minY;
            this.MaxX = maxX;
            this.MaxY = maxY;
        }

        public Envelope Extend(in Envelope other) =>
            new Envelope(
                Math.Min(this.MinX, other.MinX),
                Math.Min(this.MinY, other.MinY),
                Math.Max(this.MaxX, other.MaxX),
                Math.Max(this.MaxY, other.MaxY));

        public Envelope Clone()
        {
            return new Envelope(this.MinX, this.MinY, this.MaxX, this.MaxY);
        }

        public Envelope Intersection(in Envelope other) =>
            new Envelope(
                Math.Max(this.MinX, other.MinX),
                Math.Max(this.MinY, other.MinY),
                Math.Min(this.MaxX, other.MaxX),
                Math.Min(this.MaxY, other.MaxY)
            );

        public Envelope Enlargement(in Envelope other)
        {
            var clone = this.Clone();
            clone.Extend(other);
            return clone;
        }

        public bool Contains(in Envelope other)
        {
            return
                this.MinX <= other.MinX &&
                this.MinY <= other.MinY &&
                this.MaxX >= other.MaxX &&
                this.MaxY >= other.MaxY;
        }

        public bool Intersects(in Envelope other)
        {
            return
                this.MinX <= other.MaxX &&
                this.MinY <= other.MaxY &&
                this.MaxX >= other.MinX &&
                this.MaxY >= other.MinY;
        }

        public static Envelope InfiniteBounds =>
            new Envelope(
                double.NegativeInfinity,
                double.NegativeInfinity,
                double.PositiveInfinity,
                double.PositiveInfinity);

        public static Envelope EmptyBounds =>
            new Envelope(
                double.PositiveInfinity,
                double.PositiveInfinity,
                double.NegativeInfinity,
                double.NegativeInfinity);
    }
}