using System;
using BenchmarkDotNet.Running;

namespace Lykke.Service.HFT.Benchmarks
{
    public static class Benchmarks
    {
        public static void Main()
        {
            BenchmarkRunner.Run<HftProductionBenchmark>();

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();
        }
    }
}
