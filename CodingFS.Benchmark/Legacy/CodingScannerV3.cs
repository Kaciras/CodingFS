using System;
using System.Collections.Generic;

namespace CodingFS.Benchmark.Legacy;

sealed class CodingScannerV3
{
	public int MaxDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	readonly ConcurrentCharsDict<IReadOnlyList<Workspace>> cache = new();
	readonly Detector[] detectors;
	readonly Workspace[] globals = Array.Empty<Workspace>();

	public CodingScannerV3(string root, Detector[] detectors)
	{
		Root = root;
		this.detectors = detectors;
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);
		var part = Root.AsMemory();
		IReadOnlyList<Workspace> list = globals;
		var workspaces = new List<Workspace>(list);

		for (var limit = MaxDepth; limit > 0; limit--)
		{
			var args = (splitor.Left, workspaces);
			list = cache.GetOrAdd(part, Scan, args);

			workspaces.AddRange(list);

			if (!splitor.HasNext)
			{
				break;
			}
			part = splitor.SplitNext();
		}

		return new WorkspacesInfo(directory, workspaces, list);
	}

	List<Workspace> Scan(ReadOnlyMemory<char> _, (ReadOnlyMemory<char>, List<Workspace>) t)
	{
		var path = new string(t.Item1.Span);
		var context = new DetectContxt(path, t.Item2);

		foreach (var factory in detectors)
		{
			factory(context);
		}

		return context.Matches;
	}
}
