using System;
using System.Collections.Generic;

namespace CodingFS;

sealed class CharsDictionary<T> : Dictionary<ReadOnlyMemory<char>, T>
{
	public T this[string key]
	{
		get => base[key.AsMemory()];
		set => base[key.AsMemory()] = value;
	}

	public CharsDictionary() : base(Utils.memComparator) { }
}
