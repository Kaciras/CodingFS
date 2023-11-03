using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace CodingFS;

sealed class ConcurrentCharsDict<T> : ConcurrentDictionary<ReadOnlyMemory<char>, T>
{
	public ConcurrentCharsDict(): base(Utils.memComparator) {}
}

/// <summary>
/// Scan and cache workspaces of directories. This class is thread-safe.
/// </summary>
public sealed class CodingScanner
{
	/// <summary>
	/// Maximum search depth (include the root directory).
	/// </summary>
	public int MaxDepth { get; set; } = int.MaxValue;

	public string Root { get; }

	readonly ConcurrentCharsDict<IReadOnlyList<Workspace>> cache = new();
	readonly Detector[] detectors;
	readonly Workspace[] globals = Array.Empty<Workspace>();

	public CodingScanner(string root, Workspace[] globals, Detector[] detectors)
	{
		Root = root;
		this.globals = globals;
		this.detectors = detectors;
	}

	public WorkspacesInfo GetWorkspaces(string directory)
	{
		var splitor = new PathSpliter(directory, Root);
		IReadOnlyList<Workspace> list = globals;
		var workspaces = new List<Workspace>(list);

		for (var limit = MaxDepth; limit > 0; limit--)
		{
			list = cache.GetOrAdd(splitor.Left, Scan, workspaces);
			workspaces.AddRange(list);

			if (!splitor.HasNext)
			{
				break;
			}
			splitor.SplitNext();
		}

		return new WorkspacesInfo(directory, workspaces, list);
	}

	List<Workspace> Scan(ReadOnlyMemory<char> path, List<Workspace> workspaces)
	{
		var context = new DetectContxt(path.ToString(), workspaces);
		foreach (var factory in detectors)
		{
			factory(context);
		}

		return context.Matches;
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
