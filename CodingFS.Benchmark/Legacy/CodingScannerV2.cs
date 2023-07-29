using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CodingFS.Benchmark.Legacy;

sealed class CodingScannerV2
{
	public int MaxDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	readonly CharsDictionary<Workspace[]> cache = new();
	readonly Detector[] detectors;

	public CodingScannerV2(string root, Detector[] detectors)
	{
		Root = root;
		this.detectors = detectors;
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);

		var workspaces = new List<Workspace>(CodingScanner.GLOBALS);
		var part = Root.AsMemory();
		Workspace[] list = CodingScanner.GLOBALS;

		for (var limit = MaxDepth; limit > 0; limit--)
		{
			ref var item = ref CollectionsMarshal
				.GetValueRefOrAddDefault(cache, part, out var found);

			if (!found)
			{
				var path = new string(splitor.Left.Span);
				var ctx = new DetectContxt(path, workspaces);

				foreach (var factory in detectors)
				{
					factory(ctx);
				}

				item = list = ctx.Matches.ToArray();
			}

			workspaces.AddRange(list);

			if (!splitor.HasNext)
			{
				break;
			}
			part = splitor.SplitNext();
		}

		return new WorkspacesInfo(directory, workspaces, list);
	}
}
