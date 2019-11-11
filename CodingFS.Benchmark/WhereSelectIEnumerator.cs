using System.Collections;
using System.Collections.Generic;

namespace CodingFS.Benchmark
{
	public delegate bool TryFunc<Source, TOut>(Source source, out TOut value);

	internal struct WhereSelectIEnumerator<T, R> : IEnumerator<R>
	{
		private readonly IEnumerator<T> source;
		private readonly TryFunc<T, R> whereSelect;

		public WhereSelectIEnumerator(IEnumerator<T> source, TryFunc<T, R> whereSelect)
		{
			this.source = source;
			this.whereSelect = whereSelect;
			Current = default!;
		}

		// 不知道怎么给泛型注解上 nullable
		public R Current { get; private set; }

		object? IEnumerator.Current => Current;

		public bool MoveNext()
		{
			while (source.MoveNext())
			{
				if (whereSelect(source.Current, out var result))
				{
					Current = result;
					return true;
				}
			}
			return false;
		}

		public void Reset() => source.Reset();

		public void Dispose() => source.Dispose();
	}
}
