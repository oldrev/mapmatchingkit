using GeoAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Tests
{

    public abstract class TestBase
    {
        protected TestBase()
        {
            NetTopologySuiteBootstrapper.Bootstrap();
        }
    }
}
