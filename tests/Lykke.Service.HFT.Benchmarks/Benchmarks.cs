using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Running;
using System;

namespace Lykke.Service.HFT.Benchmarks
{
    public static class Benchmarks
    {
        public static Platform Strategy { get; private set; }
        public static object Config { get; private set; }

        public static void Main()
        {
            BenchmarkRunner.Run<HftProductionBenchmark>();

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();
        }
    }
}
