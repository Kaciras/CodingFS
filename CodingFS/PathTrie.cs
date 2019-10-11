using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodingFS.Filter;

namespace CodingFS
{
	sealed class PathTrieNode
	{
		public bool Exists { get; set; }
		public IDictionary<string, PathTrieNode>? Children { get; set; }
	}

	public sealed class PathTrie
	{
		readonly PathTrieNode root = new PathTrieNode();
		readonly RecognizeType type;

		public PathTrie(RecognizeType type)
		{
			this.type = type;
		}

		public void Add(string path)
		{
			var node = root;
			var parts = path.Split(Path.DirectorySeparatorChar);

			foreach (var part in parts)
			{
				if (node.Children == null)
				{
					node.Children = new Dictionary<string, PathTrieNode>();
				}

				if (node.Children.TryGetValue(part, out var child))
				{
					node = child;
				}
				else
				{
					var newNode = new PathTrieNode();
					node.Children.Add(part, newNode);
					node = newNode;
				}
			}

			node.Exists = true;
		}

		public RecognizeType Recognize(string path)
		{
			var node = root;
			var parts = path.Split(Path.DirectorySeparatorChar);

			foreach (var part in parts)
			{
				if (node.Children == null)
				{
					return RecognizeType.NotCare;
				}
				if (node.Children.TryGetValue(part, out var child))
				{
					node = child;
				}
				else
				{
					return RecognizeType.NotCare;
				}
			}

			return node.Exists ? type : RecognizeType.Uncertain;
		}
	}
}
