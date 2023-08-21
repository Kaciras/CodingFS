using System;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;

namespace CodingFS.Benchmark;

/**
 * |  Method |       Mean |    Error |   StdDev | Ratio |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
 * |-------- |-----------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
 * |      V1 | 2,722.3 ns | 11.43 ns | 10.69 ns |  3.30 | 0.7782 | 0.0076 |   6.37 KB |        1.40 |
 * |      V2 |   805.3 ns |  1.67 ns |  1.40 ns |  0.98 | 0.3319 | 0.0010 |   2.71 KB |        0.60 |
 * | Current |   823.8 ns | 12.14 ns | 10.76 ns |  1.00 | 0.5569 | 0.0067 |   4.55 KB |        1.00 |
 */
[ReturnValueValidator]
[MemoryDiagnoser]
public class CodingScannerPerf
{
	const string DIR = "/foo/Projects/CSharp/CodingFS/CodingFS/bin/Debug/net7.0/runtimes/win-x64/native";
	const string PATH = DIR + "/test.txt";

	static void Fac1(DetectContxt ctx) { }

	Detector[] detectors = { Fac1 };

	FileClassifierV1 NewV1()
	{
		return new FileClassifierV1("/foo", Array.Empty<Workspace>(), detectors);
	}

	CodingScannerV2 NewV2()
	{
		return new CodingScannerV2("/foo", detectors);
	}

	CodingScanner NewCurrent()
	{
		return new CodingScanner("/foo", Array.Empty<Workspace>(), detectors);
	}

	[Benchmark]
	public FileType V1_ThreadUnsafe()
	{
		return NewV1().GetWorkspaces(DIR).GetFileType(PATH);
	}

	[Benchmark]
	public FileType V2_ThreadUnsafe()
	{
		return NewV2().GetWorkspaces(DIR).GetFileType(PATH);
	}

	[Benchmark(Baseline = true)]
	public FileType Current()
	{
		return NewCurrent().GetWorkspaces(DIR).GetFileType(PATH);
	}
}
