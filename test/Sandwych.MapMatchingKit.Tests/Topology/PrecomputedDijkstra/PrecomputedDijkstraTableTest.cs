using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QuickGraph;
using Xunit;
using Sandwych.MapMatchingKit.Topology.PrecomputedDijkstra;

namespace Sandwych.MapMatchingKit.Tests.Topology.PrecomputedDijkstra
{
    public class PrecomputedDijkstraTableTest
    {
        [Theory]
        [InlineData("A", "B")]
        [InlineData("A", "J")]
        [InlineData("C", "E")]
        public void TestGetPathByVertex(string sourceVertex, string targetVertex)
        {
            var maxRadius = 500D;

            var naiveDijkstra = new BoundedDijkstraShortestPathAlgorithm<string, Edge<string>>(
                MyGraph.GraphInstance, e => MyGraph.EdgeCosts[e], maxRadius);
            naiveDijkstra.Compute(sourceVertex);

            var generator = new PrecomputedDijkstraTableGenerator<string, Edge<string>>();
            var rows = generator.ComputeRows(MyGraph.GraphInstance, e => MyGraph.EdgeCosts[e], maxRadius);

            var table = new PrecomputedDijkstraTable<string, Edge<string>>(rows);
            var actualPath = table.GetPathByVertex(sourceVertex, targetVertex);
            var actualDistance = actualPath.Sum(e => MyGraph.EdgeCosts[e]);

            Assert.True(naiveDijkstra.TryGetPath(targetVertex, out var expectedPath));
            Assert.NotNull(actualPath);
            Assert.NotEmpty(actualPath);
            var expectedDistance = expectedPath.Sum(e => MyGraph.EdgeCosts[e]);
            Assert.Equal(expectedPath, actualPath);
            Assert.Equal(expectedDistance, actualDistance, 8);
        }
    }
}
