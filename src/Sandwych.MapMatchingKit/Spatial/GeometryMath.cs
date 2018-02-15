using Sandwych.MapMatchingKit.Spatial.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial
{
    public static class GeometryMath
    {


        /// <summary>
        /// Computes the perpendicular distance from a point p
        /// to the (infinite) line containing the points AB
        /// Copy from NTS
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns>The perpendicular distance from p to line AB.</returns>
        public static double DistancePointLinePerpendicular(Coordinate2D p, Coordinate2D A, Coordinate2D B)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*(2)
                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = -----------------------------
                                         Curve^2
                        Then the distance from C to Point = |s|*Curve.
            */
            var len2 = ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
            var s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / len2;


            return Math.Abs(s) * Math.Sqrt(len2);
        }



        /// <summary>
        /// Computes the distance from a point p to a line segment AB.
        /// Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="A">One point of the line.</param>
        /// <param name="B">Another point of the line (must be different to A).</param>
        /// <returns> The distance from p to line segment AB.</returns>
        public static double DistancePointLine(Coordinate2D p, Coordinate2D A, Coordinate2D B)
        {
            // if start = end, then just compute distance to one of the endpoints
            if (A.X == B.X && A.Y == B.Y)
            {
                return p.CartesianDistance(A);
            }

            // otherwise use comp.graphics.algorithms Frequently Asked Questions method
            /*(1)     	      AC dot AB
                        r =   ---------
                              ||AB||^2
		                r has the following meaning:
		                r=0 Point = A
		                r=1 Point = B
		                r<0 Point is on the backward extension of AB
		                r>1 Point is on the forward extension of AB
		                0<r<1 Point is interior to AB
	        */

            var len2 = ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
            var r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y)) / len2;

            if (r <= 0.0)
            {
                return p.CartesianDistance(A);
            }
            if (r >= 1.0)
            {
                return p.CartesianDistance(B);
            }


            /*(2)
		                    (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = -----------------------------
		             	                Curve^2
		                Then the distance from C to Point = |s|*Curve.
                        This is the same calculation as {@link #distancePointLinePerpendicular}.
                        Unrolled here for performance.
	        */

            var s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y)) / len2;

            return Math.Abs(s) * Math.Sqrt(len2);
        }
    }
}
