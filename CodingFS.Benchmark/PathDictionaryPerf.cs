using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;

namespace CodingFS.Benchmark;

/// <summary>
/// |  Method |     Mean |   Error |  StdDev |   Gen0 | Allocated |
/// |-------- |---------:|--------:|--------:|-------:|----------:|
/// | OldImpl | 354.0 ns | 3.16 ns | 2.96 ns | 0.0324 |     272 B |
/// | NewImpl | 113.8 ns | 0.53 ns | 0.44 ns | 0.0162 |     136 B |
/// </summary>
[MemoryDiagnoser]
public class PathDictionaryPerf
{
	const string PATH = @"D:\Coding\Blog\WebServer\packages\markdown\node_modules";
	const string ROOT = @"D:\Coding";

	readonly PathDictionary old = new(ROOT);
	readonly Dictionary<ReadOnlyMemory<char>, RecognizeType> dict = new(Utils.memComparator);

	public PathDictionaryPerf()
	{
		old.AddIgnore(PATH);
		dict[PATH.AsMemory()] = RecognizeType.Ignored;
	}

	[GlobalSetup]
	public void CheckEquality()
	{
		if (OldImpl() != NewImpl())
		{
			throw new Exception("Results are not equal");
		}
	}

	[Benchmark]
	public RecognizeType OldImpl()
	{
		return old.Recognize(new string(PATH));
	}

	[Benchmark]
	public RecognizeType NewImpl()
	{
		var ns = new string(PATH);
		Utils.NormalizeSepUnsafe(ns);

		var mem = ns.AsMemory();
		return dict[mem.TrimEnd(Path.DirectorySeparatorChar)];
	}
}
