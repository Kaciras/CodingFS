using System;
using System.IO;
using System.Linq;
using Benchmark;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark;

/**
 * |   Method | SmallData |         Mean |      Error |     StdDev | Ratio | RatioSD |
 * |--------- |---------- |-------------:|-----------:|-----------:|------:|--------:|
 * | Baseline |     False |  2,220.05 ns |  27.852 ns |  26.053 ns |  1.00 |    0.00 |
 * |  Iterate |     False | 10,781.17 ns | 154.016 ns | 136.531 ns |  4.85 |    0.07 |
 * |  ForLoop |     False |  2,667.70 ns |  23.662 ns |  22.133 ns |  1.20 |    0.02 |
 * |          |           |              |            |            |       |         |
 * | Baseline |      True |     13.05 ns |   0.072 ns |   0.064 ns |  1.00 |    0.00 |
 * |  Iterate |      True |     57.93 ns |   1.189 ns |   1.667 ns |  4.46 |    0.13 |
 * |  ForLoop |      True |     12.00 ns |   0.166 ns |   0.155 ns |  0.92 |    0.01 |
 */
public class IndexOfSpanPerf
{
	const string TO_FOUND = "relate";

	[Params(true, false)]
	public bool SmallData { get; set; }

	string[] data = null!;

	[GlobalSetup]
	public void SetData()
	{
		data = SmallData
			? new[] { "Collections", "IndexOfSpanPerf", "node_modules" }
			: File.ReadAllLines(@"Resources\words.txt");
	}

	[Benchmark(Baseline = true)]
	public bool Baseline()
	{
		return Array.IndexOf(data, TO_FOUND) != -1;
	}

	[Benchmark]
	public bool Iterate()
	{
		return data.Select(x => x.AsMemory())
			.Contains(TO_FOUND.AsMemory(), Utils.memComparator);
	}

	[Benchmark]
	public bool ForLoop()
	{
		return Utils.IndexOfSpan(data, TO_FOUND) != -1;
	}
}
