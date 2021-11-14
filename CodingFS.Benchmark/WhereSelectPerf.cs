using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

// 分两步的稍微慢点，WhereSelect 的两种实现几乎没有差距（生成器的慢一点点）。
// 如果把 WhereSelectIEnumerator 改为结构体则会降低性能。
namespace CodingFS.Benchmark;

public class WhereSelectPerf
{
	private readonly int Size = 1000;
	private readonly Dictionary<int, int> dict = new Dictionary<int, int>();

	[GlobalSetup]
	public void SetUp()
	{
		for (int i = 0; i < Size; i++)
		{
			if ((i & 1) == 0)
			{
				dict[i] = i + i;
			}
		}

		int a = MyImpl(), b = WhereAndSelect(), c = Generator();
		if (a != b || b != c)
		{
			throw new Exception("几种实现的结果不相等");
		}
	}

	private static IEnumerable<R> WhereSelect2<T, R>(IEnumerable<T> source, TryFunc<T, R> func)
	{
		foreach (T item in source)
		{
			if (func(item, out R value))
			{
				yield return value;
			}
		}
	}

	[Benchmark]
	public int MyImpl()
	{
		return Enumerable.Range(0, Size).WhereSelect<int, int>(dict.TryGetValue).Sum();
	}

	[Benchmark]
	public int WhereAndSelect()
	{
		return Enumerable.Range(0, Size).Where(dict.ContainsKey).Select(k => dict[k]).Sum();
	}

	[Benchmark]
	public int Generator()
	{
		return WhereSelect2<int, int>(Enumerable.Range(0, Size), dict.TryGetValue).Sum();
	}
}
