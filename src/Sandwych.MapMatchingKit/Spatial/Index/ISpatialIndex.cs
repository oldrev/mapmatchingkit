using GeoAPI.Geometries;
using Sandwych.MapMatchingKit.Spatial.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public interface ISpatialIndex<TItem>
    {
        IEnumerable<TItem> Search(Envelope envelope);

        /// <summary>
        /// Gets objects stored in the index that are within a certain radius or overlap a certain radius.
        /// </summary>
        /// <param name="c">Center point for radius search.</param>
        /// <param name="radius">Radius in meters</param>
        /// <param name="k">maximum number of candidates</param>
        /// <returns>Result set of object(s) that are within a the given radius or overlap the radius, limited by k.</returns>
        IEnumerable<(TItem, double)> Radius(in Coordinate2D c, double radius, int k = -1);
    }
}
