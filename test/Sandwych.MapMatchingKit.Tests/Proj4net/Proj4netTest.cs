using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ProjNet;
using ProjNet.Converters;
using GeoAPI.CoordinateSystems;

//https://gis.stackexchange.com/questions/246062/epsg-4326-to-3857-works-on-x-axis-but-not-y-with-proj4net

namespace Sandwych.MapMatchingKit.Tests.Proj4net
{
    public class Proj4netTest
    {
        [Fact]
        public void Test1()
        {
            var epsg3395 = @"PROJCS[""WGS 84 / World Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Mercator_1SP""],PARAMETER[""Latitude_of_origin"", 0],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH],AUTHORITY[""EPSG"",""3395""]]";

            var epsg4326 = @"GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]]";

            var srcCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg4326, Encoding.UTF8) as ICoordinateSystem;
            var tgtCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg3395, Encoding.UTF8) as ICoordinateSystem;


            var ctFac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var trans = ctFac.CreateFromCoordinateSystems(srcCRS, tgtCRS);

            var longitude = 102.709887;
            var latitude = 25.054263;
            var fromPT = new double[] { longitude, latitude };
            var toPT = trans.MathTransform.Transform(fromPT);
            Assert.Equal(11433612.32, toPT[0], 2);
            Assert.Equal(2864322.39, toPT[1], 2);
        }
    }
}
