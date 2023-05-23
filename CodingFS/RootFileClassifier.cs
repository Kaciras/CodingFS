using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodingFS;

/// <summary>
/// 文件分类器，以给定的目录为上下文，对这之下的文件进行分类。
/// 
/// 【线程安全】
/// 该类不是线程安全的，请勿并发调用。
/// </summary>
public sealed class RootFileClassifier
{
	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	private readonly PathTrieNode<Workspace[]> rootNode;
	private readonly WorkspaceFactory[] factories;

	public RootFileClassifier(string root, Workspace[] globals, WorkspaceFactory[] factories)
	{
		Root = root;
		this.factories = factories;
		rootNode = new PathTrieNode<Workspace[]>(globals);
	}

	string[] SplitPath(string path)
	{
		var relative = Path.GetRelativePath(Root, path);
		if (relative == null)
		{
			throw new Exception("Path outside the scanner");
		}
		return relative.Split(Path.DirectorySeparatorChar);
	}

	public void Invalid(string directory)
	{
		var parts = SplitPath(directory);

		var node = rootNode;
		for (int i = 0; i < parts.Length - 1; i++)
		{
			var part = parts[i];
			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				return;
			}
		}

		node.Remove(parts[^1]);
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var parts = SplitPath(directory);

		var workspaces = new List<Workspace>();
		var node = rootNode;

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
				var matches = factories
					.Select(f => f(tempDir))
					.Where(x => x != null)!
					.ToArray<Workspace>();

				node = node.Put(part, matches);
			}

			workspaces.AddRange(node.Value);
		}

		return new WorkspacesInfo(directory, workspaces); 
	}
}
