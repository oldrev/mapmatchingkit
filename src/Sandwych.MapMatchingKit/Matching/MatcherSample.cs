using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Sandwych.MapMatchingKit.Markov;
using Sandwych.MapMatchingKit.Spatial.Geometries;

namespace Sandwych.MapMatchingKit.Matching
{
    /// <summary>
    /// Measurement sample for Hidden Markov Model (HMM) map matching which is a position measurement,
    /// e.g. measured with a GPS device.
    /// </summary>
    public readonly struct MatcherSample : ISample
    {
        public long Id { get; }
        public DateTimeOffset Time { get; }
        public float Azimuth { get; }
        public Coordinate2D Coordinate { get; }
        public bool IsNaN => this.Id < 0;
        public bool HasAzimuth => !float.IsNaN(this.Azimuth);

        public MatcherSample(long id, long time, double lng, double lat, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = DateTimeOffset.MinValue.AddMilliseconds(time);
            this.Coordinate = new Coordinate2D(lng, lat);
            this.Azimuth = NormAzimuth(azimuth);
        }

        public MatcherSample(long id, long time, in Coordinate2D point, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = DateTimeOffset.MinValue.AddMilliseconds(time);
            this.Coordinate = point;
            this.Azimuth = NormAzimuth(azimuth);
        }

        public MatcherSample(long id, DateTimeOffset time, in Coordinate2D point, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = time;
            this.Coordinate = point;
            this.Azimuth = NormAzimuth(azimuth);
        }

        public MatcherSample(long id, DateTimeOffset time, double x, double y, float azimuth = float.NaN)
        {
            this.Id = id;
            this.Time = time;
            this.Coordinate = new Coordinate2D(x, y);
            this.Azimuth = NormAzimuth(azimuth);
        }

        private static float NormAzimuth(float azimuth) =>
            azimuth >= 360f ? azimuth - (360f * (int)(azimuth / 360f))
                    : azimuth < 0f ? azimuth - (360f * ((int)(azimuth / 360f) - 1f)) : azimuth;

    }
}
