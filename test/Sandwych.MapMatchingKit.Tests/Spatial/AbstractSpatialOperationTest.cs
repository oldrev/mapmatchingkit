using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Spatial
{
    public abstract class AbstractSpatialOperationTest : TestBase
    {
        protected abstract ISpatialOperation Spatial { get; }

        protected (Coordinate2D, double, double) Intercept(Coordinate2D a, Coordinate2D b, Coordinate2D c)
        {
            int iter = 1000;

            var res = (a, Spatial.Distance(a, c), 0d);

            for (int f = 1; f <= iter; ++f)
            {

                var p = Spatial.Interpolate(a, b, (double)f / iter);
                double s = Spatial.Distance(p, c);

                if (s < res.Item2)
                {

                    res.Item1 = p;
                    res.Item2 = s;
                    res.Item3 = (double)f / iter;
                }
            }
            return res;
        }

    }
}
