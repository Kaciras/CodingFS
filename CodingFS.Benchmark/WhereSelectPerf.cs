using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

// 分两步的稍微慢点，WhereSelect 两种实现几乎没有差距
namespace CodingFS.Benchmark
{
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
}
