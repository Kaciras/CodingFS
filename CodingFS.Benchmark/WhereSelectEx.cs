using System.Collections;
using System.Collections.Generic;

namespace CodingFS.Benchmark
{
	internal static class WhereSelectEx
	{
		/// <summary>
		/// C# 有个模式叫 Try-Pattern，比如字典的 TryGetValue 同时有转换和过滤的功能，
		/// 但对于这一类方法Linq包里没有合适的函数来处理，只能使用 Where 和 Select：
		/// <code>
		/// [...].Where(dict.ContainsKey).Select(key => dict[key]);
		/// </code>
		/// 上面的代码检索了字典两次，浪费时间，故写了这个扩展方法专门处理 Try-Pattern。
		/// </summary>
		/// <typeparam name="T">输入元素类型</typeparam>
		/// <typeparam name="R">输出元素类型</typeparam>
		/// <param name="source">输入迭代器</param>
		/// <param name="func">映射函数</param>
		/// <returns>过滤和转换元素后的迭代器</returns>
		public static IEnumerable<R> WhereSelect<T, R>(this IEnumerable<T> source, TryFunc<T, R> func)
		{
			return new WhereSelectEnumerable<T, R>(source, func);
		}

		private class WhereSelectEnumerable<T, R> : IEnumerable<R>
		{
			private readonly IEnumerable<T> source;
			private readonly TryFunc<T, R> whereSelect;

			public WhereSelectEnumerable(IEnumerable<T> source, TryFunc<T, R> whereSelect)
			{
				this.source = source;
				this.whereSelect = whereSelect;
			}

			public IEnumerator<R> GetEnumerator()
			{
				return new WhereSelectIEnumerator<T, R>(source.GetEnumerator(), whereSelect);
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
