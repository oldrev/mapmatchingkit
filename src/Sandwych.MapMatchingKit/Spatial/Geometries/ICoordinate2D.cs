using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Geometries
{
    public interface ICoordinate2D : IComparable<ICoordinate2D>
    {
        double X { get; }
        double Y { get; }
        double this[int index] { get; }
        bool IsNan { get; }
    }

    public static class ICoordinate2DExtensions
    {

        public static int CompareTo<T>(this T self, T other)
            where T : ICoordinate2D
        {
            if (self.X < other.X)
            {
                return -1;
            }
            if (self.X > other.X)
            {
                return 1;
            }
            if (self.Y < other.Y)
            {
                return -1;
            }
            return self.Y > other.Y ? 1 : 0;
        }
    }
}
