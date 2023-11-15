using System;
using System.Collections.Generic;

namespace CodingFS.Benchmark.Legacy;

sealed class CodingScannerV3(string root, Detector[] detectors)
{
	readonly struct TrieNode(IReadOnlyList<Workspace> value)
	{
		public readonly ConcurrentCharsDict<TrieNode> Children = new();
		public readonly IReadOnlyList<Workspace> Value = value;
	}

	public int MaxDepth { get; set; } = int.MaxValue;

	public string Root { get; } = root;

	readonly Detector[] detectors = detectors;
	readonly Workspace[] globals = [];
	readonly TrieNode cacheRoot = new([]);

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);
		var cacheRootLocal = cacheRoot;
		var part = ReadOnlyMemory<char>.Empty;
		var workspaces = new List<Workspace>(cacheRootLocal.Value);

		ref var node = ref cacheRootLocal;
		for (var limit = MaxDepth; limit > 0; limit--)
		{
			var args = (splitor.Left, workspaces);
			node = node.Children.GetOrAdd(part, NewNode, args);

			workspaces.AddRange(node.Value);
			if (!splitor.HasNext)
			{
				break;
			}
			part = splitor.SplitNext();
		}

		return new WorkspacesInfo(directory, workspaces, node.Value);
	}

	TrieNode NewNode(ReadOnlyMemory<char> _, (ReadOnlyMemory<char>, List<Workspace>) t)
	{
		var ctx = new DetectContxt(
			new string(t.Item1.Span),
			t.Item2
		);
		foreach (var x in detectors)
		{
			x(ctx);
		}
		return new TrieNode(ctx.Matches);
	}
}
