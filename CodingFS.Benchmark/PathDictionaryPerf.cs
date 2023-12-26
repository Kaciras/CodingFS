using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;
using CodingFS.Helper;

namespace CodingFS.Benchmark;

/// <summary>
/// | Method      | Mean      | StdDev   | Ratio | Gen0   | Allocated |
/// |------------ |----------:|---------:|------:|-------:|----------:|
/// | OldImpl     | 319.74 ns | 0.670 ns |  3.85 | 0.0324 |     272 B |
/// | NoNormalize |  61.09 ns | 0.149 ns |  0.74 | 0.0162 |     136 B |
/// | NewImpl     |  83.11 ns | 0.152 ns |  1.00 | 0.0162 |     136 B |
/// </summary>
[ReturnValueValidator]
[MemoryDiagnoser]
public class PathDictionaryPerf
{
	const string PATH = @"D:\Coding\Blog\WebServer\packages\markdown\node_modules";
	const string ROOT = @"D:\Coding";

	readonly PathDictionary old = new(ROOT);
	readonly CharsDictionary<RecognizeType> dict = new();

	public PathDictionaryPerf()
	{
		old.AddIgnore(PATH);
		dict[PATH.AsMemory()] = RecognizeType.Ignored;
	}

	[Benchmark]
	public RecognizeType OldImpl()
	{
		return old.Recognize(new string(PATH));
	}

	[Benchmark]
	public RecognizeType NoNormalize()
	{
		var mem = new string(PATH).AsMemory();
		return dict[mem.TrimEnd(Path.DirectorySeparatorChar)];
	}

	[Benchmark(Baseline = true)]
	public RecognizeType NewImpl()
	{
		var ns = new string(PATH);
		PathSpliter.NormalizeSepUnsafe(ns);

		var mem = ns.AsMemory();
		return dict[mem.TrimEnd(Path.DirectorySeparatorChar)];
	}
}
