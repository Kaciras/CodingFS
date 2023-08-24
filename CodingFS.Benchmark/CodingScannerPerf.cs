using System;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;

namespace CodingFS.Benchmark;

/**
 * |          Method |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
 * |---------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
 * | V1_ThreadUnsafe | 2,596.8 ns | 19.25 ns | 18.00 ns |  2.14 |    0.02 | 0.7744 | 0.0076 |   6.33 KB |        1.80 |
 * | V2_ThreadUnsafe |   741.2 ns |  4.16 ns |  3.69 ns |  0.61 |    0.00 | 0.3233 | 0.0010 |   2.65 KB |        0.75 |
 * |              V3 | 1,878.6 ns | 18.49 ns | 17.30 ns |  1.55 |    0.02 | 1.5469 | 0.0629 |  12.64 KB |        3.60 |
 * |         Current | 1,210.7 ns |  9.38 ns |  8.78 ns |  1.00 |    0.00 | 0.4292 | 0.0019 |   3.52 KB |        1.00 |
 */
[MemoryDiagnoser]
public class CodingScannerPerf
{
	const string DIR = "/foo/Projects/CSharp/CodingFS/CodingFS/bin/Debug/net7.0/runtimes/win-x64/native";

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

	CodingScannerV3 NewV3()
	{
		return new CodingScannerV3("/foo", detectors);
	}

	CodingScanner NewCurrent()
	{
		return new CodingScanner("/foo", Array.Empty<Workspace>(), detectors);
	}

	[Benchmark]
	public WorkspacesInfo V1_ThreadUnsafe()
	{
		return NewV1().GetWorkspaces(DIR);
	}

	[Benchmark]
	public WorkspacesInfo V2_ThreadUnsafe()
	{
		return NewV2().GetWorkspaces(DIR);
	}

	[Benchmark]
	public WorkspacesInfo V3()
	{
		return NewV3().GetWorkspaces(DIR);
	}

	[Benchmark(Baseline = true)]
	public WorkspacesInfo Current()
	{
		return NewCurrent().GetWorkspaces(DIR);
	}
}
