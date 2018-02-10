using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Projection
{
    public sealed class Epsg3395To4326CoordinateTransformation : AbstractWktCoordinateTransformation
    {
        public Epsg3395To4326CoordinateTransformation() : base(WellKnownConstants.Epsg3395, WellKnownConstants.Epsg4326)
        {

        }
    }
}
