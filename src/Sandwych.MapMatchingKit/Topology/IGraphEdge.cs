using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Topology
{
    /// <summary>
    /// The interface of edge in a directed <see cref="Topology.IGraph{TEdge}"/>.
    /// </summary>
    /// <typeparam name="T">Implementation of <see cref="Topology.IGraphEdge{T}" /> in a directed graph. 
    /// (Use the curiously recurring template pattern (CRTP) for type-safe use of customized edge type.)
    /// </typeparam>
    public interface IGraphEdge<T>
        where T : IGraphEdge<T>
    {
        /// <summary>
        /// Gets the edge's identifier.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Gets the edge's source vertex.
        /// </summary>
        long Source { get; }

        /// <summary>
        /// Gets the edge's target vertex. 
        /// </summary>
        long Target { get; }

        /// <summary>
        /// Gets the edge's neighbor.
        /// </summary>
        T Neighbor { get; }

        /// <summary>
        /// Gets the edge's successor.
        /// </summary>
        T Successor { get; }

        /// <summary>
        /// Gets the edge's successor edges.
        /// </summary>
        IEnumerable<T> Successors { get; }
    }
}
