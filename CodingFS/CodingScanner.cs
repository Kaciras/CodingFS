using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CodingFS.Workspaces;
namespace CodingFS;

/// <summary>
/// Scan and cache workspaces of directories. This class is not thread-safe.
/// </summary>
public sealed class CodingScanner
{
	readonly struct TrieNode
	{
		public readonly CharsDictionary<TrieNode> Children = new();
		public readonly IReadOnlyList<Workspace> Value;

		public TrieNode(IReadOnlyList<Workspace> value) { Value = value; }
	}

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
	/// Maximum search depth (include the root directory).
	/// </summary>
	public int MaxDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	readonly Detector[] detectors;
	readonly TrieNode cacheRoot;

	public CodingScanner(string root) : this(root, GLOBALS, DETECTORS) { }

	public CodingScanner(string root, Workspace[] globals) : this(root, globals, DETECTORS) { }

	public CodingScanner(string root, Workspace[] globals, Detector[] detectors)
	{
		Root = root;
		this.detectors = detectors;
		cacheRoot = new TrieNode(globals);
	}

	public void InvalidCache(string directory)
	{
		var splitor = new PathSpliter(directory, Root);
		var node = cacheRoot;
		var part = ReadOnlyMemory<char>.Empty;

		for (; ; )
		{
			if (node.Children.TryGetValue(part, out var child))
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

		node.Children.Remove(part);
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);
		var node = cacheRoot;
		var part = ReadOnlyMemory<char>.Empty;
		var workspaces = new List<Workspace>(node.Value);

		for (var limit = MaxDepth; limit > 0; limit--)
		{
			ref var child = ref CollectionsMarshal
				.GetValueRefOrAddDefault(node.Children, part, out var exists);

			if (exists)
			{
				node = child;
			}
			else
			{
				var path = new string(splitor.Left.Span);
				var ctx = new DetectContxt(path, workspaces);

				foreach (var detector in detectors)
				{
					detector(ctx);
				}

				child = node = new(ctx.Matches);
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

	public IEnumerable<(FileSystemInfo, FileType)> Walk(FileType includes)
	{
		return Walk(new DirectoryInfo(Root), includes);
	}

	public IEnumerable<(FileSystemInfo, FileType)> Walk(string folder, FileType includes)
	{
		return Walk(new DirectoryInfo(folder), includes);
	}

	// Enumerating FileSystemInfo does not produce more IO operations than enumerating path.
	// https://github.com/dotnet/runtime/blob/485e4bf291285e281f1d8ff8861bf9b7a7827c64/src/libraries/System.Private.CoreLib/src/System/IO/Enumeration/FileSystemEnumerableFactory.cs
	// https://github.com/dotnet/runtime/blob/485e4bf291285e281f1d8ff8861bf9b7a7827c64/src/libraries/System.Private.CoreLib/src/System/IO/FileSystemInfo.Windows.cs#L26
	public IEnumerable<(FileSystemInfo, FileType)> Walk(DirectoryInfo folder, FileType includes)
	{
		var info = GetWorkspaces(folder.FullName);
		foreach (var file in folder.EnumerateFileSystemInfos())
		{
			var type = info.GetFileType(file.FullName);
			if (!includes.HasFlag(type))
			{
				continue;
			}
			yield return (file, type);

			if (file is DirectoryInfo next)
			{
				foreach (var x in Walk(next, includes)) yield return x;
			}
		}
	}
}
