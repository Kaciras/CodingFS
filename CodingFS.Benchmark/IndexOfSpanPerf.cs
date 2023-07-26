using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark;

public class IndexOfSpanPerf
{
	const string TO_FOUND = "ByEnumerable";

	static readonly string[] data =
	{
		"CodingFS",
		"Benchmark",
		"namespace",
		"using",
		"System",
		"Collections",
		"Generic",
		"Linq",
		"Text",
		"Threading",
		"Tasks",
		"BenchmarkDotNet",
		"Attributes",
		"public",
		"class",
		"ByEnumerable",
		"IndexOfSpanPerf",
	};

	[Benchmark(Baseline = true)]
	public bool Baseline()
	{
		return Array.IndexOf(data, TO_FOUND) != -1;
	}

	[Benchmark]
	public bool ByEnumerable()
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
