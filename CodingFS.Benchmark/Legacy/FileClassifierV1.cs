using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace CodingFS.Benchmark.Legacy;

file struct PathTrieNode<T>
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

	public readonly void Remove(string part)
	{
		children?.Remove(part);
	}

	public PathTrieNode<T> Put(string part, T value)
	{
		children ??= new Dictionary<string, PathTrieNode<T>>();
		return children[part] = new PathTrieNode<T>(value);
	}
}

internal sealed class FileClassifierV1
{
	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	private readonly WorkspaceFactory[] factories;
	private readonly PathTrieNode<Workspace[]> cacheRoot;

	public FileClassifierV1(string root) : this(root, FileClassifier.GLOBALS, FileClassifier.FACTORIES) { }

	public FileClassifierV1(string root, Workspace[] globals, WorkspaceFactory[] factories)
	{
		Root = root;
		this.factories = factories;
		cacheRoot = new PathTrieNode<Workspace[]>(globals);
	}

	string[] SplitPath(string path)
	{
		var relative = Path.GetRelativePath(Root, path);
		return relative == null
			? throw new Exception("Path outside the scanner")
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
				var matches = new List<Workspace>();
				var ctx = new DetectContxt(workspaces, tempDir, matches);

				foreach (var factory in factories)
				{
					factory(ctx);
				}

				node = node.Put(part, matches.ToArray());
			}

			workspaces.AddRange(node.Value);
		}

		return new WorkspacesInfo(directory, workspaces, node.Value);
	}
}
