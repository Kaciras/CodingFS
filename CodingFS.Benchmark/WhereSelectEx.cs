using System.Collections;
using System.Collections.Generic;

namespace CodingFS.Benchmark
{
	internal static class WhereSelectEx
	{
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
