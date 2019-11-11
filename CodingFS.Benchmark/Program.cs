using System;
using BenchmarkDotNet.Running;

namespace CodingFS.Benchmark
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<WhereSelectPerf>();
		}
	}
}
