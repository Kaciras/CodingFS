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

		public bool TryGetChild(string part, [MaybeNullWhen(false)] out PathTrieNode<T>? child)
		{
			if (children == null)
			{
				child = null;
				return false;
			}
			return children.TryGetValue(part, out child);
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

	public sealed class PathTrie<T>
	{
		readonly PathTrieNode<T> root;
		readonly T defaultValue;

		public PathTrie(T defaultValue)
		{
			root = new PathTrieNode<T>(defaultValue);
			this.defaultValue = defaultValue;
		}

		public void Add(string path, T value)
		{
			var node = root;
			var parts = path.Split(Path.DirectorySeparatorChar);

			foreach (var part in parts)
			{
				if (node.TryGetChild(part, out var child))
				{
					node = child!;
				}
				else
				{
					var newNode = new PathTrieNode<T>(defaultValue);
					node.PutChild(part, newNode);
					node = newNode;
				}
			}

			node.Value = value;
		}

		public T Get(string path, T alternative)
		{
			var node = root;
			var parts = path.Split(Path.DirectorySeparatorChar);

			foreach (var part in parts)
			{
				if (node.TryGetChild(part, out var child))
				{
					node = child!;
				}
				else
				{
					return alternative;
				}
			}

			return node.Value;
		}
	}
}
