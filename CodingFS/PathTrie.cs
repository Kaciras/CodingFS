using System.Collections.Generic;
using System.IO;

namespace CodingFS
{
	sealed class PathTrieNode<T>
	{
		public T Value { get; set; }
		public IDictionary<string, PathTrieNode<T>>? Children { get; set; }

		public PathTrieNode(T value) => Value = value;
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
				if (node.Children == null)
				{
					node.Children = new Dictionary<string, PathTrieNode<T>>();
				}

				if (node.Children.TryGetValue(part, out var child))
				{
					node = child;
				}
				else
				{
					var newNode = new PathTrieNode<T>(defaultValue);
					node.Children.Add(part, newNode);
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
				var children = node.Children;

				if (children == null)
				{
					return alternative;
				}
				if (children.TryGetValue(part, out var child))
				{
					node = child;
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
