using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    public interface ISpatialOperation
    {
        /// <summary>
        /// Gets the distance between two <see cref="Geometries.Coordinate2D"/> <i>a</i> and <i>b</i>.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>Distance between points in meters.</returns>
        double Distance(in Coordinate2D a, in Coordinate2D b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        double Length(ILineString line);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        double Length(IMultiLineString line);

        /// <summary>
        /// <para>Gets interception point of a LineString intercepted by Point c. </para>
        /// <para>This is analog to <see cref="Intercept(Coordinate2D, Coordinate2D, Coordinate2D)"/>. </para>
        /// <para>The fraction <i>f</i> refers to the full length of the LineString.</para>
        /// </summary>
        /// <param name="p">Line to be intercepted.</param>
        /// <param name="c">Point that intercepts straight line a to b.</param>
        /// <returns>Interception point described as the linearly interpolated fraction f in the interval [0,1] of the line</returns>
        double Intercept(ILineString p, in Coordinate2D c);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        double Intercept(in Coordinate2D a, in Coordinate2D b, in Coordinate2D c);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        Coordinate2D Interpolate(in Coordinate2D a, in Coordinate2D b, double f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        Coordinate2D Interpolate(ILineString path, double f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        Coordinate2D Interpolate(ILineString path, double l, double f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        double Azimuth(in Coordinate2D a, in Coordinate2D b, double f);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        double Azimuth(ILineString path, double f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="l"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        double Azimuth(ILineString path, double l, double f);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        Envelope Envelope(in Coordinate2D c, double radius);


        Envelope Envelope(ILineString line);

    }
}
