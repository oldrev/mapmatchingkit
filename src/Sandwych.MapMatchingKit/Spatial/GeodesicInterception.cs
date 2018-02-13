/*
 * Copyright (C) 2015, BMW Car IT GmbH
 *
 * Author: Sebastian Mattheis <sebastian.mattheis@bmw-carit.de>
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in
 * writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
 * language governing permissions and limitations under the License.
 */

/**
 * Implementation of the net.sf.geographiclib.Intercept class
 */

using System;
using System.Collections.Generic;
using System.Text;
using GeographicLib;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Spatial
{
    /*
 * <p>
 * <i>Note: Intercept.java has been ported to Java from its C++ equivalent Intercept.cpp, authored
 * by C. F. F. Karney and licensed under MIT/X11 license. The following documentation is mostly the
 * same as for its C++ equivalent, but has been adopted to apply to this Java implementation.</i>
 * <p>
 * Simple solution to the interception using the gnomonic projection. The interception problem is,
 * given a geodesic <i>a</i> and a point <i>b</i>, determine the point <i>p</i> on the geodesic
 * <i>a</i> that is closest to point <i>b</i>. The gnomonic projection and the solution to the
 * interception problem are derived in Section 8 of
 * <ul>
 * <li>C. F. F. Karney, <a href="http://dx.doi.org/10.1007/s00190-012-0578-z"> Algorithms for
 * geodesics</a>, J. Geodesy <b>87</b>, 43--55 (2013); DOI:
 * <a href="http://dx.doi.org/10.1007/s00190-012-0578-z"> 10.1007/s00190-012-0578-z</a>; addenda:
 * <a href="http://geographiclib.sf.net/geod-addenda.html"> geod-addenda.html</a>.</li>
 * </ul>
 * <p>
 * In gnomonic projection geodesics are nearly straight; and they are exactly straight if they go
 * through the center of projection. The interception can then be found as follows: Guess an
 * interception point. Project the resulting line segments into gnomonic, compute their intersection
 * in this projection, use this intersection point as the new center, and repeat.
 * <p>
 * <b>CAUTION:</b> The solution to the interception problem is valid only under the following
 * conditions:
 * <ul>
 * <li>The two points defining the geodesic and the point of interception must be in the same
 * hemisphere centered at the interception point for the gnomonic projection to be defined.</li>
 * </ul>
 */

    /// <summary>
    /// Geodesic interception.
    /// </summary>
    public readonly struct GeodesicInterception
    {

        private static readonly double eps = 0.01 * Math.Sqrt(GeoMath.Epsilon);
        /**
         * Maximum number of iterations for calculation of interception point. (The solution should
         * usually converge before reaching the maximum number of iterations. The default is 10.)
         */
        public static int _maxit = 10;
        private readonly Gnomonic _gnom;

        /**
         * Constructor for Intercept.
         * <p>
         *
         * @param earth the {@link Geodesic} object to use for geodesic calculations. By default the
         *        WGS84 ellipsoid should be used.
         */
        public GeodesicInterception(Geodesic earth)
        {
            this._gnom = new Gnomonic(earth);
        }

        /**
         * Interception of a point <i>b</i> to a geodesic <i>a</i>.
         * <p>
         *
         * @param lata1 latitude of point <i>1</i> of geodesic <i>a</i> (degrees).
         * @param lona1 longitude of point <i>1</i> of geodesic <i>a</i> (degrees).
         * @param lata2 latitude of point <i>2</i> of geodesic <i>a</i> (degrees).
         * @param lona2 longitude of point <i>2</i> of geodesic <i>a</i> (degrees).
         * @param latb1 latitude of point <i>b</i> (degrees).
         * @param lonb1 longitude of point <i>b</i> (degrees).
         * @return a {@link GeodesicData} object, defining a geodesic from point <i>b</i> to the
         *         intersection point, with the following fields: <i>lat1</i>, <i>lon1</i>, <i>azi1</i>,
         *         <i>lat2</i>, <i>lon2</i>, <i>azi2</i>, <i>s12</i>, <i>a12</i>.
         *         <p>
         *         <i>lat1</i> should be in the range [&minus;90&deg;, 90&deg;]; <i>lon1</i> and
         *         <i>azi1</i> should be in the range [&minus;540&deg;, 540&deg;). The values of
         *         <i>lon2</i> and <i>azi2</i> returned are in the range [&minus;180&deg;, 180&deg;).
         */
        public GeodesicData Intercept(double lata1, double lona1, double lata2, double lona2,
                double latb1, double lonb1)
        {

            if (lata1 == lata2 && lona1 == lona2)
            {
                return _gnom.Earth.Inverse(latb1, lonb1, lata1, lona1);
            }

            var inv = Geodesic.WGS84.Inverse(lata1, lona1, lata2, lona2);
            var est = Geodesic.WGS84.Line(inv.lat1, inv.lon1, inv.azi1).Position(inv.s12 * 0.5);
            double latb2 = est.lat2, latb2_ = double.NaN, lonb2_ = double.NaN, lonb2 = est.lon2;

            for (int i = 0; i < _maxit; ++i)
            {
                var xa1 = _gnom.Forward(latb2, lonb2, lata1, lona1);
                var xa2 = _gnom.Forward(latb2, lonb2, lata2, lona2);
                var xb1 = _gnom.Forward(latb2, lonb2, latb1, lonb1);

                var va1 = new Vector3D(xa1.x, xa1.y, 1);
                var va2 = new Vector3D(xa2.x, xa2.y, 1);
                var la = va1.Cross(va2);
                var lb = new Vector3D(la.Y, -(la.X), la.X * xb1.y - la.Y * xb1.x);
                var p0 = la.Cross(lb);
                p0 = p0 * (1d / p0.Z);

                latb2_ = latb2;
                lonb2_ = lonb2;

                GnomonicData rev = _gnom.Reverse(latb2, lonb2, p0.X, p0.Y);
                latb2 = rev.lat;
                lonb2 = rev.lon;

                if (Math.Abs(lonb2_ - lonb2) < eps && Math.Abs(latb2_ - latb2) < eps)
                {
                    break;
                }
            }

            return _gnom.Earth.Inverse(latb1, lonb1, latb2, lonb2);
        }
    }

}
