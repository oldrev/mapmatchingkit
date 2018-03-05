using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace Sandwych.MapMatchingKit.BenchmarkApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RoutersBenchmark>();
        }
    }
}
