using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CodingFS;

// TODO: 改成更省内存的数据结构
public struct PathTrieNode<T>
{
	private IDictionary<string, PathTrieNode<T>>? children;

	public T Value { get; set; }

	public PathTrieNode(T value)
	{
		Value = value;
	}

	public bool TryGet(string part, [MaybeNullWhen(false)] out PathTrieNode<T> child)
	{
		if (children == null)
		{
			child = default!;
			return false;
		}
		return children.TryGetValue(part, out child!);
	}

	public void Remove(string part)
	{
		children?.Remove(part);
	}

	public PathTrieNode<T> Put(string part, T value)
	{
		children ??= new Dictionary<string, PathTrieNode<T>>();
		return children[part] = new PathTrieNode<T>(value);
	}
}
