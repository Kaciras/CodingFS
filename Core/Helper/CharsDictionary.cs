using System;
using System.Collections.Generic;

namespace CodingFS.Helper;

/// <summary>
/// The dictionary uses ReadOnlyMemory&lt;char&gt; as it key.
/// </summary>
/// <typeparam name="T">The value type</typeparam>
/// <see href="https://github.com/dotnet/runtime/issues/27229"/>
public sealed class CharsDictionary<T> : Dictionary<ReadOnlyMemory<char>, T>
{
	public T this[string key]
	{
		get => base[key.AsMemory()];
		set => base[key.AsMemory()] = value;
	}

	public CharsDictionary() : base(Utils.memComparator) { }

	public bool Remove(string key) => Remove(key.AsMemory());
}
