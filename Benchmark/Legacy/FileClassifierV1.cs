using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace CodingFS.Benchmark.Legacy;

sealed class PathTrieNode<T>(T value)
{
	Dictionary<string, PathTrieNode<T>>? children;

	public T Value { get; set; } = value;

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
		children ??= new();
		return children[part] = new PathTrieNode<T>(value);
	}
}

internal sealed class FileClassifierV1(string root, Workspace[] globals, Detector[] factories)
{
	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; } = root;

	private readonly Detector[] factories = factories;
	private readonly PathTrieNode<Workspace[]> cacheRoot = new(globals);

	string[] SplitPath(string path)
	{
		var relative = Path.GetRelativePath(Root, path);
		return relative == null
			? throw new Exception("Directory outside the scanner")
			: relative.Split(Path.DirectorySeparatorChar);
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var parts = SplitPath(directory);
		var node = cacheRoot;
		var workspaces = new List<Workspace>(node.Value);

		for (int i = 0; i < parts.Length; i++)
		{
			var part = parts[i];

			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				var tempDir = Path.Join(Root, string.Join('\\', parts.Take(i + 1)));
				var ctx = new DetectContxt(tempDir, workspaces);

				foreach (var factory in factories)
				{
					factory(ctx);
				}

				node = node.Put(part, ctx.Matches.ToArray());
			}

			workspaces.AddRange(node.Value);
		}

		return new WorkspacesInfo(directory, workspaces, node.Value);
	}
}
