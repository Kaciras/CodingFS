using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CodingFS.Helper;

namespace CodingFS.Benchmark;

/*
 * | Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
 * |------- |---------:|----------:|----------:|-------:|----------:|
 * | String | 2.065 us | 0.0409 us | 0.0845 us | 0.1965 |    1656 B |
 * | Memory | 1.456 us | 0.0290 us | 0.0500 us | 0.1163 |     976 B |
 */
[MemoryDiagnoser]
public class CharMemoryHashPerf
{
	const string PATH = @"Projects\CSharp\CodingFS\CodingFS\bin\Debug\net7.0\runtimes\win-x64\native";

	[Benchmark]
	public object String()
	{
		var hashSet = new HashSet<string>();
		var ancestor = PATH;
		while (ancestor != null)
		{
			hashSet.Add(ancestor);
			ancestor = Path.GetDirectoryName(ancestor);
		}
		return hashSet;
	}

	[Benchmark]
	public object Memory()
	{
		var hashSet = new HashSet<ReadOnlyMemory<char>>(Utils.memComparator);
		var ancestor = PATH.AsMemory();
		while (ancestor.Length > 0)
		{
			hashSet.Add(ancestor);
			var s = Path.GetDirectoryName(ancestor.Span);
			ancestor = ancestor[..s.Length];
		}
		return hashSet;
	}
}
