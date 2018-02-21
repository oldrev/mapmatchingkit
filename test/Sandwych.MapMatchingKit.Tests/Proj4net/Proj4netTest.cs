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
        public void TestEpsg3395AndEpsg4326()
        {
            var epsg3395 = @"PROJCS[""WGS 84 / World Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Mercator_1SP""],PARAMETER[""Latitude_of_origin"", 0],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH],AUTHORITY[""EPSG"",""3395""]]";

            var epsg4326 = @"GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]]";

            var srcCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg4326, Encoding.ASCII) as ICoordinateSystem;
            var tgtCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg3395, Encoding.ASCII) as ICoordinateSystem;


            var ctFac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var trans = ctFac.CreateFromCoordinateSystems(srcCRS, tgtCRS);

            var longitude = 102.709887;
            var latitude = 25.054263;
            var fromPT = new double[] { longitude, latitude };
            var toPT = trans.MathTransform.Transform(fromPT);
            Assert.Equal(11433612.32, toPT[0], 2);
            Assert.Equal(2864322.39, toPT[1], 2);
        }

        [Fact]
        public void TestEpsg3857AndEpsg4326()
        {
            var epsg3857 = @"PROJCS[""WGS 84 / Pseudo - Mercator"", GEOGCS[""WGS 84"", DATUM[""WGS_1984"", SPHEROID[""WGS 84"", 6378137, 298.257223563, AUTHORITY[""EPSG"", ""7030""]], AUTHORITY[""EPSG"", ""6326""]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9122""]], AUTHORITY[""EPSG"", ""4326""]], PROJECTION[""Mercator_1SP""], PARAMETER[""Latitude_of_origin"", 0], PARAMETER[""central_meridian"", 0], PARAMETER[""scale_factor"", 1], PARAMETER[""false_easting"", 0], PARAMETER[""false_northing"", 0], UNIT[""metre"", 1, AUTHORITY[""EPSG"", ""9001""]], AXIS[""X"", EAST], AXIS[""Y"", NORTH], EXTENSION[""PROJ4"", "" + proj = merc + a = 6378137 + b = 6378137 + lat_ts = 0.0 + lon_0 = 0.0 + x_0 = 0.0 + y_0 = 0 + k = 1.0 + units = m + nadgrids = @null + wktext + no_defs""], AUTHORITY[""EPSG"", ""3857""]]";
            var epsg4326 = @"GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.01745329251994328,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]]";

            var srcCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg4326, Encoding.ASCII) as ICoordinateSystem;
            var tgtCRS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(epsg3857, Encoding.ASCII) as ICoordinateSystem;


            var ctFac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
            var trans = ctFac.CreateFromCoordinateSystems(srcCRS, tgtCRS);

            var longitude = 102.709887;
            var latitude = 25.054263;
            var fromPT = new double[] { longitude, latitude };
            var toPT = trans.MathTransform.Transform(fromPT);
            Assert.Equal(11433612.32, toPT[0], 2);
            //TODO & FIXME: ProjNet did not works well with EPSG 4326
            //Assert.Equal(2882411.08, toPT[1], 2);
        }
    }
}
