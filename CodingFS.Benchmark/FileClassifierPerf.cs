using System;
using BenchmarkDotNet.Attributes;
using CodingFS.Benchmark.Legacy;

namespace CodingFS.Benchmark;

/// <summary>
/// |        Method |       Mean |    Error |   StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
/// |-------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
/// | V1_StringPath | 2,581.6 ns | 18.09 ns | 16.92 ns |  1.00 | 0.7744 |   6.34 KB |        1.00 |
/// |   CurrentImpl |   812.7 ns | 12.01 ns | 11.23 ns |  0.31 | 0.5169 |   4.23 KB |        0.67 |
/// 
/// |        Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
/// |-------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
/// | V1_StringPath | 759.5 ns | 3.05 ns | 2.85 ns |  1.00 | 0.1297 |    1088 B |        1.00 |
/// |   CurrentImpl | 431.0 ns | 1.42 ns | 1.33 ns |  0.57 | 0.0238 |     200 B |        0.18 |
/// </summary>
[MemoryDiagnoser]
public class FileClassifierPerf
{
	const string DIR = "/foo/Projects/CSharp/CodingFS/CodingFS/bin/Debug/net7.0/runtimes/win-x64/native";
	const string PATH = DIR + "/test.txt";

	static void Fac1(DetectContxt ctx)	{}

	readonly FileClassifierV1 filter = new("/foo", Array.Empty<Workspace>(), new WorkspaceFactory[] { Fac1 });
	readonly CodingPathFilter filter2 = new("/foo", Array.Empty<Workspace>(), new WorkspaceFactory[] { Fac1 });

	[Benchmark(Baseline = true)]
	public FileType V1_StringPath()
	{
		return filter.GetWorkspaces(DIR).GetFileType(PATH);
	}

	[Benchmark]
	public FileType CurrentImpl()
	{
		return filter2.GetWorkspaces(DIR).GetFileType(PATH);
	}
}
