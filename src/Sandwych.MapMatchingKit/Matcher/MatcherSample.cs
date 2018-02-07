using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Matcher
{
    /// <summary>
    /// Measurement sample for Hidden Markov Model (HMM) map matching which is a position measurement,
    /// e.g. measured with a GPS device.
    /// </summary>
    /// <typeparam name="TSampleId"></typeparam>
    public readonly struct MatcherSample<TSampleId> : ISample
    {
        public TSampleId Id { get; }
        public long Time { get; }
        public float Azimuth { get; }
        public Coordinate2D Coordinate { get; }

        public MatcherSample(TSampleId id, long time, double lng, double lat, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = time;
            this.Coordinate = new Coordinate2D(lng, lat);
            this.Azimuth = NormAzimuth(azimuth);
        }

        public MatcherSample(TSampleId id, long time, Coordinate2D point, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = time;
            this.Coordinate = point;
            this.Azimuth = NormAzimuth(azimuth);
        }

        private static float NormAzimuth(float azimuth) =>
            azimuth >= 360f ? azimuth - (360f * (int)(azimuth / 360f))
                    : azimuth < 0f ? azimuth - (360f * ((int)(azimuth / 360f) - 1f)) : azimuth;

    }
}
