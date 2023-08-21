using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using DokanNet;

namespace CodingFS.Benchmark;

file sealed class NopPathFilter : PathFilter
{
	public void HandleChange(string path) { }

	public IEnumerable<FileInformation> ListFiles(string _)
	{
		return Array.Empty<FileInformation>();
	}

	public string MapPath(string path) => string.Empty;
}

/**
 * Enumerate a List(43 ns) is faster than Dictionory.Keys(3.2 us).
 * 
 * |       Method |     Mean |    Error |   StdDev |   Gen0 | Allocated |
 * |------------- |---------:|---------:|---------:|-------:|----------:|
 * | HandleChange | 53.07 ns | 0.076 ns | 0.071 ns | 0.0067 |      56 B |
 * |    ListFiles | 43.18 ns | 0.108 ns | 0.101 ns | 0.0239 |     200 B |
 */
[MemoryDiagnoser]
public class MappedPathFilterPerf
{
	readonly MappedPathFilter filter = new();

	string filename = null!;

	[GlobalSetup]
	public void SetUp()
	{
		var buffer = (stackalloc byte[8]);
		var name = string.Empty;
		var inner = new NopPathFilter();

		for (int i = 0; i < 100; i++)
		{
			Random.Shared.NextBytes(buffer);
			name = Convert.ToHexString(buffer);
			filter.Set(name, inner);
		}

		filename = $@"\{name}\foo\bar\baz.txt";
	}

	[Benchmark]
	public void HandleChange()
	{
		filter.HandleChange(filename);
	}

	[Benchmark]
	public FileInformation ListFiles()
	{
		return filter.ListFiles(@"\").Last();
	}
}
