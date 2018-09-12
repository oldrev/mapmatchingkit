using Sandwych.MapMatchingKit.Topology;
using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace Sandwych.MapMatchingKit.Roads
{

    /// <summary>
    /// Directed road wrapper of <see cref="Roads.RoadInfo"/> objects in a directed road map (<see cref="RoadMap"/>).
    /// <para>
    /// <b>Note:</b> Since <see cref="Road"/> objects are directional representations of <see cref="Roads.RoadInfo"/>
    /// objects, each <see cref="Roads.RoadInfo"/> is split into two <see cref="Road"/> objects. For that purpose, it uses
    /// the identifier <i>i</i> of each <see cref="Roads.RoadInfo"/> to define identifiers of the respective
    /// {@link Road} objects, where <i>i * 2</i> is the identifier of the forward directed <see cref="Road"/>
    /// and <i>i * 2 + 1</i> of the backward directed <see cref="Road"/>.
    /// </para>
    /// </summary>
    public class Road : AbstractGraphEdge<Road>
    {
        /// <summary>
        /// Gets referred BaseRoad object.
        /// </summary>
        public RoadInfo RoadInfo { get; }

        /// <summary>
        /// Gets road Heading relative to its RoadInfo.
        /// </summary>
        public Heading Headeing { get; }

        /// <summary>
        /// Gets road's geometry as a <see cref="GeoAPI.Geometries.ILineString"/> from the road's source to its target.
        /// </summary>
        public ILineString Geometry { get; }


        /// <summary>
        /// Constructs Road object.
        /// </summary>
        /// <param name="info">RoadInfo object to be referred to</param>
        /// <param name="heading">Heading of the directed Road</param>
        public Road(RoadInfo info, Heading heading) :
            base(heading == Heading.Forward ? info.Id * 2 : info.Id * 2 + 1,
                heading == Heading.Forward ? info.Source : info.Target,
                heading == Heading.Forward ? info.Target : info.Source)
        {
            this.RoadInfo = info;
            this.Headeing = heading;
            if (heading == Heading.Forward)
            {
                this.Geometry = info.Geometry;
            }
            else
            {
                this.Geometry = info.Geometry.Reverse() as ILineString;
            }
        }

        /// <summary>
        /// Gets road length in meters.
        /// </summary>
        public float Length => this.RoadInfo.Length;

        /// <summary>
        /// Gets road's maximum speed in kilometers per hour.
        /// </summary>
        public float MaxSpeed => this.Headeing == Heading.Forward ? this.RoadInfo.MaxSpeedForward : this.RoadInfo.MaxSpeedBackward;

        /// <summary>
        /// Gets road's priority factor, i.e. an additional cost factor for routing, and must be greater
        /// or equal to one.Higher priority factor means higher costs.
        /// </summary>
        public float Priority => this.RoadInfo.Priority;
    }
}
