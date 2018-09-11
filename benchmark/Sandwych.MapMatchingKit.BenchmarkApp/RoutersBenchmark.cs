using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using BenchmarkDotNet.Attributes;
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

namespace Sandwych.MapMatchingKit.BenchmarkApp
{
    [RPlotExporter, HtmlExporter, RankColumn]
    [MemoryDiagnoser, ShortRunJob]
    public class RoutersBenchmark
    {
        private const double MaxDistance = 1000D;
        private const double MaxGpsRadius = 100D;
        private string DataDirPath { get; set; }

        private RoadMap _roadMap;
        private Matcher<MatcherCandidate, MatcherTransition, MatcherSample> _naiveDijkstraMatcher;
        private Matcher<MatcherCandidate, MatcherTransition, MatcherSample> _precomputedDijkstraMatcher;

        private MatcherSample[] _samples;

        [GlobalSetup]
        public void Setup()
        {
            this.DataDirPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../../../../../../", "data"));

            var spatial = new GeographySpatialOperation();
            var roads = this.ReadRoads(spatial);

            var mapBuilder = new RoadMapBuilder(spatial);
            _roadMap = mapBuilder.AddRoads(roads).Build();

            {
                var naiveDijkstraRouter = new DijkstraRouter<Road, RoadPoint>();
                _naiveDijkstraMatcher = new Matcher<MatcherCandidate, MatcherTransition, MatcherSample>(
                    _roadMap, naiveDijkstraRouter, Costs.TimePriorityCost, spatial);
                _naiveDijkstraMatcher.MaxDistance = MaxDistance; // set maximum searching distance between two GPS points to 1000 meters.
                _naiveDijkstraMatcher.MaxRadius = MaxGpsRadius; // sets maximum radius for candidate selection to 200 meters
            }

            {
                var precomputedDijkstraRouter = new PrecomputedDijkstraRouter<Road, RoadPoint>(_roadMap, Costs.TimePriorityCost, Costs.DistanceCost, MaxDistance);
                _precomputedDijkstraMatcher = new Matcher<MatcherCandidate, MatcherTransition, MatcherSample>(
                    _roadMap, precomputedDijkstraRouter, Costs.TimePriorityCost, spatial);
                _precomputedDijkstraMatcher.MaxDistance = MaxDistance; // set maximum searching distance between two GPS points to 1000 meters.
                _precomputedDijkstraMatcher.MaxRadius = MaxGpsRadius; // sets maximum radius for candidate selection to 200 meters
            }

            _samples = ReadSamples().OrderBy(s => s.Time).ToArray();
        }

        [Benchmark(Baseline = true)]
        public int NaiveDijkstraMatching()
        {
            return this.DoMatching(_naiveDijkstraMatcher);
        }

        [Benchmark]
        public int PrecomputedDijkstraMatching()
        {
            return this.DoMatching(_precomputedDijkstraMatcher);
        }

        private int DoMatching(Matcher<MatcherCandidate, MatcherTransition, MatcherSample> matcher)
        {
            var kstate = new MatcherKState();
            foreach (var sample in _samples)
            {
                var vector = matcher.Execute(kstate.Vector(), kstate.Sample, sample);
                kstate.Update(vector, sample);
            }
            return 0;
        }

        private IEnumerable<MatcherSample> ReadSamples()
        {
            var json = File.ReadAllText(System.IO.Path.Combine(DataDirPath, @"samples.geojson"));
            var reader = new GeoJsonReader();
            var fc = reader.Read<FeatureCollection>(json);
            var timeFormat = "yyyy-MM-dd-HH.mm.ss";
            var samples = new List<MatcherSample>();
            foreach (var i in fc.Features)
            {
                var p = i.Geometry as IPoint;
                var coord2D = new Coordinate2D(p.X, p.Y);
                var timeStr = i.Attributes["time"].ToString().Substring(0, timeFormat.Length);
                var time = DateTimeOffset.ParseExact(timeStr, timeFormat, CultureInfo.InvariantCulture);
                var longTime = time.ToUnixTimeMilliseconds();
                yield return new MatcherSample(longTime, time, coord2D);
            }
        }


        private IEnumerable<RoadInfo> ReadRoads(ISpatialOperation spatial)
        {
            var json = File.ReadAllText(Path.Combine(DataDirPath, @"osm-kunming-roads-network.geojson"));
            var reader = new GeoJsonReader();
            var fc = reader.Read<FeatureCollection>(json);
            foreach (var feature in fc.Features)
            {
                var lineGeom = feature.Geometry as ILineString;
                yield return new RoadInfo(
                    Convert.ToInt64(feature.Attributes["gid"]),
                    Convert.ToInt64(feature.Attributes["source"]),
                    Convert.ToInt64(feature.Attributes["target"]),
                    (double)feature.Attributes["reverse"] >= 0D ? false : true,
                    (short)0,
                    Convert.ToSingle(feature.Attributes["priority"]),
                    120f,
                    120f,
                    Convert.ToSingle(spatial.Length(lineGeom)),
                    lineGeom);
            }
        }


    }
}
