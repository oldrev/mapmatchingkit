using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Matching;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Spatial;
using Sandwych.MapMatchingKit.Topology;
using Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra;
using Sandwych.MapMatchingKit.Spatial.Index;
using Sandwych.MapMatchingKit.Spatial.Index.RBush;

namespace Sandwych.MapMatchingKit.BenchmarkApp
{
    [RPlotExporter, HtmlExporter, RankColumn]
    [MemoryDiagnoser, ShortRunJob]
    public class SpatialIndexBenchmark
    {
        private const double MaxDistance = 1000D;
        private const double MaxGpsRadius = 100D;
        private string DataDirPath { get; set; }

        private ISpatialOperation Spatial = new GeographySpatialOperation();
        private RtreeIndex<ILineString> _ntsRtreeIndex;
        private QuadtreeIndex<ILineString> _ntsQuadtreeIndex;
        private RBushSpatialIndex<ILineString> _rbushIndex;

        protected IReadOnlyList<ILineString> _geometries;

        static SpatialIndexBenchmark()
        {
            GeoAPI.NetTopologySuiteBootstrapper.Bootstrap();
        }

        [GlobalSetup]
        public void Setup()
        {
            _geometries = this.MakeGeometries();
            _ntsRtreeIndex = new RtreeIndex<ILineString>(_geometries, this.Spatial, x => x, x => this.Spatial.Length(x));
            _ntsQuadtreeIndex = new QuadtreeIndex<ILineString>(_geometries, this.Spatial, x => x, x => this.Spatial.Length(x));
            _rbushIndex = new RBushSpatialIndex<ILineString>(_geometries, this.Spatial, x => x, x => this.Spatial.Length(x));
        }

        [Benchmark]
        public void NtsSTRTreeBenchmark()
        {
            this.DoRadius(_ntsRtreeIndex);
        }

        [Benchmark(Baseline = true)]
        public void NtsQuadTreeBenchmark()
        {
            this.DoRadius(_ntsQuadtreeIndex);
        }

        [Benchmark]
        public void RBushRtreeBenchmark()
        {
            this.DoRadius(_rbushIndex);
        }

        private int DoRadius(ISpatialIndex<ILineString> index)
        {
            {
                var c = new Coordinate2D(11.343629, 48.083797);
                var r = 50;
                index.Radius(c, r);
            }
            {
                var c = new Coordinate2D(11.344827, 48.083752);
                var r = 10;
                index.Radius(c, r);
            }
            {
                var c = new Coordinate2D(11.344827, 48.083752);
                var r = 5;
                index.Radius(c, r);
            }
            return 0;
        }

        private IReadOnlyList<ILineString> MakeGeometries()
        {
            /*
             * (p2) (p3) ----- (e1) : (p1) -> (p2) ----------------------------------------------------
             * - \ / --------- (e2) : (p3) -> (p1) ----------------------------------------------------
             * | (p1) | ------ (e3) : (p4) -> (p1) ----------------------------------------------------
             * - / \ --------- (e4) : (p1) -> (p5) ----------------------------------------------------
             * (p4) (p5) ----- (e5) : (p2) -> (p4) ----------------------------------------------------
             * --------------- (e6) : (p5) -> (p3) ----------------------------------------------------
             */
            String p1 = "11.3441505 48.0839963";
            String p2 = "11.3421209 48.0850624";
            String p3 = "11.3460348 48.0850108";
            String p4 = "11.3427522 48.0832129";
            String p5 = "11.3469701 48.0825356";
            var reader = new WKTReader();
            ILineString readAsLineString(string wkt) => reader.Read("SRID=4326;" + wkt) as ILineString;

            var geometries = new ILineString[] {
                readAsLineString("LINESTRING(" + p1 + "," + p2 + ")"),
                readAsLineString("LINESTRING(" + p3 + "," + p1 + ")"),
                readAsLineString("LINESTRING(" + p4 + "," + p1 + ")"),
                readAsLineString("LINESTRING(" + p1 + "," + p5 + ")"),
                readAsLineString("LINESTRING(" + p2 + "," + p4 + ")"),
                readAsLineString("LINESTRING(" + p5 + "," + p3 + ")"),
            };

            var geoms = new List<ILineString>();
            for (int i = 0; i < geometries.Length; i++)
            {
                var g = geometries[i];
                g.UserData = i;
                geoms.Add(g);
            }

            return geoms;
        }


    }
}
