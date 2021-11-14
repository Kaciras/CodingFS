using System;
using BenchmarkDotNet.Running;

namespace CodingFS.Benchmark;

internal class Program
{
	private static void Main(string[] args)
	{
		BenchmarkRunner.Run<FSWrapperPerf>();
	}
}
