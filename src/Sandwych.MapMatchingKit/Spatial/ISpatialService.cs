using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    public interface ISpatialService
    {
        /// <summary>
        /// Gets the distance between two {@link Point}s <i>a</i> and <i>b</i>.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Distance between points in meters.</returns>
        double Distance(in Coordinate2D a, in Coordinate2D b);

        double Length(ILineString line);

        double Length(IMultiLineString line);

        /// <summary>
        /// Gets interception point of a LineString intercepted by Point c. 
        /// This is analog to {@link SpatialOperator#intercept(Point, Point, Point)}. 
        /// The fraction <i>f</i> refers to the full length of the LineString.
        /// </summary>
        /// <param name="p">Line to be intercepted.</param>
        /// <param name="c">Point that intercepts straight line a to b.</param>
        /// <returns>Interception point described as the linearly interpolated fraction f in the interval [0,1] of the line</returns>
        double Intercept(ILineString p, in Coordinate2D c);

        double Intercept(in Coordinate2D a, in Coordinate2D b, in Coordinate2D p);

        Coordinate2D Interpolate(in Coordinate2D a, in Coordinate2D b, double f);

        Coordinate2D Interpolate(ILineString path, double f);

        Coordinate2D Interpolate(ILineString path, double l, double f);

        double Azimuth(in Coordinate2D a, in Coordinate2D b, double f);

        double Azimuth(ILineString path, double f);

        double Azimuth(ILineString path, double l, double f);

        Envelope Envelope(in Coordinate2D c, double radius);

    }
}
