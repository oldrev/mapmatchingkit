using System;
using System.IO;
using System.Linq;
using System.Globalization;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Sandwych.MapMatchingKit.Matching;
using System.Collections.Generic;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using Sandwych.MapMatchingKit.Roads;
using Sandwych.MapMatchingKit.Topology;
using Sandwych.MapMatchingKit.Spatial;
using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra;

namespace Sandwych.MapMatchingKit.Examples.HelloWorldApp
{
    class Program
    {
        private static readonly string s_dataDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "data"));

        static void Main(string[] args)
        {
            var spatial = new GeographySpatialOperation();
            var mapBuilder = new RoadMapBuilder(spatial);

            Console.WriteLine("Loading road map...");
            var roads = ReadRoads(spatial);
            var map = mapBuilder.AddRoads(roads).Build();
            Console.WriteLine("The road map has been loaded");


            //var router = new PrecomputedDijkstraRouter<Road, RoadPoint>(map, Costs.TimePriorityCost, Costs.DistanceCost, 1000D);
            var router = new DijkstraRouter<Road, RoadPoint>();

            var matcher = new Matcher<MatcherCandidate, MatcherTransition, MatcherSample>(
                map, router, Costs.TimePriorityCost, spatial);
            matcher.MaxDistance = 1000; // set maximum searching distance between two GPS points to 1000 meters.
            matcher.MaxRadius = 200.0; // sets maximum radius for candidate selection to 200 meters


            Console.WriteLine("Loading GPS samples...");
            var samples = ReadSamples().OrderBy(s => s.Time).ToList();
            Console.WriteLine("GPS samples loaded. [count={0}]", samples.Count);

            Console.WriteLine("Starting Offline map-matching...");
            OfflineMatch(matcher, samples);


            Console.WriteLine("Starting Online map-matching...");
            //Uncomment below line to see how online-matching works
            //OnlineMatch(matcher, samples);

            Console.WriteLine("All done!");
            Console.ReadKey();
        }

        private static void OnlineMatch(
            Matcher<MatcherCandidate, MatcherTransition, MatcherSample> matcher,
            IReadOnlyList<MatcherSample> samples)
        {
            // Create initial (empty) state memory
            var kstate = new MatcherKState();

            // Iterate over sequence (stream) of samples
            foreach (var sample in samples)
            {
                // Execute matcher with single sample and update state memory
                var vector = kstate.Vector();
                vector = matcher.Execute(vector, kstate.Sample, sample);
                kstate.Update(vector, sample);

                // Access map matching result: estimate for most recent sample
                var estimated = kstate.Estimate();
                Console.WriteLine("RoadID={0}", estimated.Point.Edge.RoadInfo.Id); // The id of the road in your map
            }
        }

        private static void OfflineMatch(
            Matcher<MatcherCandidate, MatcherTransition, MatcherSample> matcher,
            IReadOnlyList<MatcherSample> samples)
        {
            var kstate = new MatcherKState();

            //Do the offline map-matching
            Console.WriteLine("Doing map-matching...");
            var startedOn = DateTime.Now;
            foreach (var sample in samples)
            {
                var vector = matcher.Execute(kstate.Vector(), kstate.Sample, sample);
                kstate.Update(vector, sample);
            }

            Console.WriteLine("Fetching map-matching results...");
            var candidatesSequence = kstate.Sequence();
            var timeElapsed = DateTime.Now - startedOn;
            Console.WriteLine("Map-matching elapsed time: {0}, Speed={1} samples/second", timeElapsed, samples.Count / timeElapsed.TotalSeconds);
            Console.WriteLine("Results: [count={0}]", candidatesSequence.Count());
            var csvLines = new List<string>();
            csvLines.Add("time,lng,lat,azimuth");
            int matchedCandidateCount = 0;
            foreach (var cand in candidatesSequence)
            {
                var roadId = cand.Point.Edge.RoadInfo.Id; // original road id
                var heading = cand.Point.Edge.Headeing; // heading
                var coord = cand.Point.Coordinate; // GPS position (on the road)
                csvLines.Add(string.Format("{0},{1},{2},{3}", cand.Sample.Time.ToUnixTimeSeconds(), coord.X, coord.Y, cand.Point.Azimuth));
                if (cand.HasTransition)
                {
                    var geom = cand.Transition.Route.ToGeometry(); // path geometry(LineString) from last matching candidate
                    //cand.Transition.Route.Edges // Road segments between two GPS position
                }
                matchedCandidateCount++;
            }
            Console.WriteLine("Matched Candidates: {0}, Rate: {1}%", matchedCandidateCount, matchedCandidateCount * 100 / samples.Count());

            var csvFile = System.IO.Path.Combine(s_dataDir, "samples.output.csv");
            Console.WriteLine("Writing output file: {0}", csvFile);
            File.WriteAllLines(csvFile, csvLines);
        }


        private static IEnumerable<MatcherSample> ReadSamples()
        {
            var json = File.ReadAllText(System.IO.Path.Combine(s_dataDir, @"samples.oneday.geojson"));
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


        private static IEnumerable<RoadInfo> ReadRoads(ISpatialOperation spatial)
        {
            var json = File.ReadAllText(Path.Combine(s_dataDir, @"osm-kunming-roads-network.geojson"));
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
