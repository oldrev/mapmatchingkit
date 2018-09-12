using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Tests.Topology
{
    public class RouterTestData_SameRoad : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RouterTestData_SameRoad()
        {
            var roads = new Road[] {
                    new Road(0, 0, 1, 100),
                    new Road(1, 1, 0, 20),
                    new Road(2, 0, 2, 100),
                    new Road(3, 1, 2, 100),
                    new Road(4, 1, 3, 100)
                };
            var map = new Graph(roads);
            Func<Road, double> cost = e => e.Weight;


            _data = new List<object[]>()
            {
                new object[] {
                    map,
                    new long[] { 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L, 1L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 1L, 0L },
                    new RoadPoint[] 
                    {
                        new RoadPoint(map.EdgeMap[0], 0.8),
                        new RoadPoint(map.EdgeMap[1L], 0.2)
                    },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    cost, null, double.NaN
                },
            };
        }

    }


    public class RouterTestData_SelfLoop : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RouterTestData_SelfLoop()
        {
            var roads = new Road[] {
                new Road(0, 0, 0, 100),
                new Road(1, 0, 0, 100),
            };
            var map = new Graph(roads);
            Func<Road, double> cost = e => e.Weight;

            _data = new List<object[]>()
            {
                new object[] {
                    map,
                    new long[] { 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.8), new RoadPoint(map.EdgeMap[1L], 0.2) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.2) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 1L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.4), new RoadPoint(map.EdgeMap[1L], 0.6) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

            };
        }

    }


    public class RouterTestData_ShortestPath : IEnumerable<object[]>
    {
        private readonly List<object[]> _data;

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RouterTestData_ShortestPath()
        {
            var roads = new Road[] {
                new Road(0, 0, 1, 100),
                new Road(1, 1, 0, 100),
                new Road(2, 0, 2, 160),
                new Road(3, 2, 0, 160),
                new Road(4, 1, 2, 50),
                new Road(5, 2, 1, 50),
                new Road(6, 1, 3, 200),
                new Road(7, 3, 1, 200),
                new Road(8, 2, 3, 100),
                new Road(9, 3, 2, 100),
                new Road(10, 2, 4, 40),
                new Road(11, 4, 2, 40),
                new Road(12, 3, 4, 100),
                new Road(13, 4, 3, 100),
                new Road(14, 3, 5, 200),
                new Road(15, 5, 3, 200),
                new Road(16, 4, 5, 60),
                new Road(17, 5, 4, 60),
            };
            var map = new Graph(roads);
            Func<Road, double> cost = e => e.Weight;

            _data = new List<object[]>()
            {
                new object[] {
                    map,
                    new long[] { 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.7) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 0L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.8), new RoadPoint(map.EdgeMap[1L], 0.2) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.2) },
                    cost, null, double.NaN
                },

                new object[] {
                    map,
                    new long[] { 1L, 0L },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.4), new RoadPoint(map.EdgeMap[1L], 0.6) },
                    new RoadPoint[] { new RoadPoint(map.EdgeMap[0L], 0.3) },
                    cost, null, double.NaN
                },

            };
        }

    }


}
