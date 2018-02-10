using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Projection
{
    public sealed class Epsg4326To3395CoordinateTransformation : AbstractWktCoordinateTransformation
    {

        public Epsg4326To3395CoordinateTransformation() : base(WellKnownConstants.Epsg4326, WellKnownConstants.Epsg3395)
        {

        }


    }
}
