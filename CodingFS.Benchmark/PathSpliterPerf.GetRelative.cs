using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using CodingFS.Helper;

namespace CodingFS.Benchmark;

public class PathSpliterPerf
{
	string path = @"Projects\CSharp\CodingFS\CodingFS\bin\Debug\net7.0\runtimes\win-x64\native";
	string relativeTo = @"Projects\CSharp\CodingFS";

	[Params(true, false)]
	public bool FullBase { get; set; }

	[Params(true, false)]
	public bool FullPath { get; set; }

	[GlobalSetup]
	public void GlobalSetup()
	{
		if (FullBase)
		{
			relativeTo = Path.GetFullPath(relativeTo);
		}
		if (FullPath)
		{
			path = Path.GetFullPath(path);
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
