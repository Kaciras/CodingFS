using System;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;

namespace CodingFS.Benchmark;

/**
 * |          Method |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
 * |---------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
 * | V1_ThreadUnsafe | 2,519.0 ns |  5.40 ns |  4.22 ns |  2.67 |    0.01 | 0.7744 | 0.0076 |   6.33 KB |        1.80 |
 * | V2_ThreadUnsafe |   745.8 ns |  7.74 ns |  7.24 ns |  0.79 |    0.01 | 0.3233 | 0.0010 |   2.65 KB |        0.76 |
 * |              V3 | 2,585.3 ns | 50.80 ns | 66.05 ns |  2.73 |    0.07 | 1.5450 | 0.0610 |  12.64 KB |        3.60 |
 * |         Current |   942.3 ns |  2.20 ns |  1.84 ns |  1.00 |    0.00 | 0.4292 | 0.0019 |   3.51 KB |        1.00 |
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
