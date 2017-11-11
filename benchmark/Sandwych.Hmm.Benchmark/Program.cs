using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace Sandwych.Hmm.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HmmForwardAlgorithm>();
            Console.ReadKey();
        }
    }
}
