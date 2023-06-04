using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark;

/// <summary>
/// |        Method |       Mean |   Error |  StdDev |   Gen0 | Allocated |
/// |-------------- |-----------:|--------:|--------:|-------:|----------:|
/// | V1_StringPath | 1,494.4 ns | 5.82 ns | 4.86 ns | 0.4292 |   3.51 KB |
/// |   GetFileType |   505.1 ns | 5.58 ns | 5.22 ns | 0.2871 |   2.35 KB |
/// </summary>
[MemoryDiagnoser]
public class FileClassifierPerf
{
	const string DIR = "/foo/Projects/CSharp/CodingFS/CodingFS/bin/Debug/net7.0/runtimes/win-x64/native";
	const string PATH = DIR + "/test.txt";

	static void Fac1(DetectContxt ctx)
	{

	}

	readonly FileClassifierV1 filter = new("/foo", new WorkspaceFactory[] { Fac1 }, Array.Empty<Workspace>());
	readonly FileClassifier filter2 = new("/foo", new WorkspaceFactory[] { Fac1 }, Array.Empty<Workspace>());

	[Benchmark]
	public FileType V1_StringPath()
	{
		return filter.GetWorkspaces(DIR).GetFileType(PATH);
	}

	[Benchmark]
	public FileType GetFileType()
	{
		return filter2.GetWorkspaces(DIR).GetFileType(PATH);
	}
}
