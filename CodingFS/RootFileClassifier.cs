using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodingFS.Workspaces;

namespace CodingFS;

/// <summary>
/// 文件分类器，以给定的目录为上下文，对这之下的文件进行分类。
/// 
/// 【线程安全】
/// 该类不是线程安全的，请勿并发调用。
/// </summary>
public sealed class RootFileClassifier
{
	public static readonly WorkspaceFactory[] FACTORIES =
	{
		new JetBrainsDetector().Detect,
		NpmWorkspace.Match,
		GitWorkspace.Match,
		MavenWorkspace.Match,
		CargoWorkspace.Match,
		VSCodeWorkspace.Match,
		VisualStudioWorkspace.Match,
	};

	public static readonly Workspace[] GLOBALS =
	{
		new CustomWorkspace(),
		new CommonWorkspace(),
	};

	// ===============================================================

	public int OuterDepth { get; set; } = int.MaxValue;

	public int InnerDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	private readonly WorkspaceFactory[] factories;
	private readonly PathTrieNode<Workspace[]> cacheRoot;

	public RootFileClassifier(string root): this(root, FACTORIES, GLOBALS) {}

	public RootFileClassifier(string root, WorkspaceFactory[] factories, Workspace[] globals)
	{
		Root = root;
		this.factories = factories;
		cacheRoot = new PathTrieNode<Workspace[]>(globals);
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

		var node = cacheRoot;
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

	
	public IEnumerable<(string, FileType)> Walk(string root, FileType includes)
	{
		// EnumerateFiles 和 EnumerateDirectories 都是在这基础上过滤的。
		foreach (var file in Directory.EnumerateFileSystemEntries(root))
		{
			var folder = Path.GetDirectoryName(file)!;
			var type = GetWorkspaces(folder).GetFileType(file);

			if (!includes.HasFlag(type))
			{
				continue;
			}
			yield return (file, type);

			if (Directory.Exists(file))
			{
				foreach (var i in Walk(file, includes)) yield return i;
			}
		}
	}
}
