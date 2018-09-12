using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Spatial.Index;
using Sandwych.MapMatchingKit.Spatial.Index.RBush;

namespace Sandwych.MapMatchingKit.Tests.Spatial.Index
{
    public class RBushIndexTest : AbstractSpatialIndexTest
    {
        protected override ISpatialIndex<ILineString> CreateSpatialIndex() =>
            new RBushSpatialIndex<ILineString>(this.Geometries, this.Spatial, x => x, x => this.Spatial.Length(x));
    }
}
