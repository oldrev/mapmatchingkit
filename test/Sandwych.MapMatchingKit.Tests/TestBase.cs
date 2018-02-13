using GeoAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests
{
    public abstract class TestBase
    {
        protected TestBase()
        {
            NetTopologySuiteBootstrapper.Bootstrap();
        }

        protected static void AssertEquals(double actual, double expected, double delta) =>
            Assert.InRange(actual, expected - delta, expected + delta);

    }
}
