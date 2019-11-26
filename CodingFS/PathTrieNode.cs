using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CodingFS
{
	// TODO: 改成更省内存的数据结构
	public sealed class PathTrieNode<T>
	{
		public T Value { get; set; }

		private IDictionary<string, PathTrieNode<T>>? children;

		public PathTrieNode(T value)
		{
			Value = value;
		}

		public bool TryGetChild(string part, [MaybeNullWhen(false)] out PathTrieNode<T> child)
		{
			if (children == null)
			{
				child = default!;
				return false;
			}
			return children.TryGetValue(part, out child!);
		}

		public PathTrieNode<T> PutChild(string part, T value)
		{
			var node = new PathTrieNode<T>(value);
			PutChild(part, node);
			return node;
		}

		public void PutChild(string part, PathTrieNode<T> child)
		{
			if (children == null)
			{
				children = new Dictionary<string, PathTrieNode<T>>();
			}
			children[part] = child;
		}
	}
}
