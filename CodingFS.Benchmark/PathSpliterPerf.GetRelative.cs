using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace CodingFS.Benchmark;

public class PathSpliterPerf
{
	string path = @"Projects\CSharp\CodingFS\CodingFS\bin\Debug\net7.0\runtimes\win-x64\native";
	string relativeTo = @"Projects\CSharp\CodingFS";

	[Params(true, false)]
	public bool Fullly { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		if (Fullly)
		{
			path = Path.GetFullPath(path);
			relativeTo = Path.GetFullPath(relativeTo);
		}
	}

	[Benchmark]
	public ReadOnlySpan<char> GetRelativeCLR()
	{
		return Path.GetRelativePath(relativeTo, path);
	}

	[Benchmark]
	public ReadOnlySpan<char> GetRelative()
	{
		return PathSpliter.GetRelative(relativeTo, path);
	}
}
