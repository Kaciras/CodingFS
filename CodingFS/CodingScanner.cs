using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CodingFS.Workspaces;

namespace CodingFS;

file class TrieNode<T>
{
	Dictionary<ReadOnlyMemory<char>, TrieNode<T>>? children;

	public T Value { get; set; }

	public TrieNode(T value)
	{
		Value = value;
	}

	public bool TryGet(
		ReadOnlyMemory<char> part,
		[MaybeNullWhen(false)] out TrieNode<T> child)
	{
		if (children == null)
		{
			child = default!;
			return false;
		}
		return children.TryGetValue(part, out child!);
	}

	public void Remove(ReadOnlyMemory<char> part)
	{
		children?.Remove(part);
	}

	public TrieNode<T> Put(ReadOnlyMemory<char> part, T value)
	{
		children ??= new(Utils.memComparator);
		return children[part] = new TrieNode<T>(value);
	}
}

/// <summary>
/// Scan and cache workspaces of directories. This class is not thread-safe.
/// </summary>
public sealed class CodingScanner
{
	/// <summary>
	/// Directory concains VCS repository or IDE config is a project root.
	/// </summary>
	const WorkspaceKind PROJECT = WorkspaceKind.IDE | WorkspaceKind.VCS;

	/// <summary>
	/// Maximum supported components length in file path.
	/// </summary>
	public const int MAX_COMPONENT = 255;

	public static readonly Detector[] DETECTORS =
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
		new CommonWorkspace(),
	};

	/// <summary>
	/// Maximum search depth from the Root to project.
	/// </summary>
	public int ProjectDepth { get; set; } = int.MaxValue;

	/// <summary>
	/// Maximum search depth from project to sub module.
	/// </summary>
	public int ModuleDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	readonly Detector[] detectors;
	readonly TrieNode<Workspace[]> cacheRoot;

	public CodingScanner(string root): this(root, GLOBALS, DETECTORS) {}

	public CodingScanner(string root, Workspace[] globals) : this(root, globals, DETECTORS) { }

	public CodingScanner(string root, Workspace[] globals, Detector[] detectors)
	{
		Root = root;
		this.detectors = detectors;
		cacheRoot = new TrieNode<Workspace[]>(globals);
	}

	public void InvalidCache(string directory)
	{
		var splitor = new PathSpliter(directory, Root);

		var node = cacheRoot;
		var part = Root.AsMemory();

		for (; ; )
		{
			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				return;
			}
			if (!splitor.HasNext)
			{
				break;
			}
			part = splitor.SplitNext();
		}

		node.Remove(part);
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);

		var node = cacheRoot;
		var workspaces = new List<Workspace>(node.Value);
		var part = Root.AsMemory();

		for (; ; )
		{
			if (node.TryGet(part, out var child))
			{
				node = child;
			}
			else
			{
				var path = new string(splitor.Left.Span);
				var matches = new List<Workspace>();
				var ctx = new DetectContxt(workspaces, path, matches);

				foreach (var factory in detectors)
				{
					factory(ctx);
				}

				node = node.Put(part, matches.ToArray());
			}
			workspaces.AddRange(node.Value);

			if (!splitor.HasNext)
			{
				break;
			}
			part = splitor.SplitNext();
		}

		return new WorkspacesInfo(directory, workspaces, node.Value); 
	}

	public IEnumerable<(string, FileType)> Walk(string root, FileType includes)
	{
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
				foreach (var x in Walk(file, includes)) yield return x;
			}
		}
	}
}
