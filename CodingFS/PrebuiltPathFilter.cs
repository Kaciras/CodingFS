using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodingFS.Helper;
using DokanNet;

namespace CodingFS;

/**
 * 
 */
public class PrebuiltPathFilter : PathFilter
{
	readonly Dictionary<string, List<FileSystemInfo>> map = new();
	readonly HashSet<ReadOnlyMemory<char>> matches = new (Utils.memComparator);

	readonly CodingScanner scanner;
	readonly FileType includes;
	readonly int maxDepth;

	public PrebuiltPathFilter(CodingScanner scanner, FileType includes, int maxDepth)
	{
		this.scanner = scanner;
		this.includes = includes;
		this.maxDepth = maxDepth;

		BuildMap(new DirectoryInfo(scanner.Root), maxDepth);
	}

	bool BuildMap(DirectoryInfo directory, int limit)
	{
		var ws = scanner.GetWorkspaces(directory.FullName);
		var files = new List<FileSystemInfo>();

		foreach (var e in directory.EnumerateFileSystemInfos())
		{
			var type = ws.GetFileType(e.FullName);
			var included = (type & includes) != 0;

			var hasChildren = type == FileType.Source
				&& e is DirectoryInfo next
				&& limit > 0
				&& BuildMap(next, limit - 1);

			if (included || hasChildren)
			{
				files.Add(e);
			}
			if (included && e is DirectoryInfo)
			{
				matches.Add(e.FullName.AsMemory());
			}
		}

		if (files.Count > 0)
		{
			map[directory.FullName] = files;
			return true;
		}
		return false;
	}

	public string MapPath(string path)
	{
		return Path.Join(scanner.Root, path);
	}

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		var depth = dir.AsSpan().Count(Path.DirectorySeparatorChar);
		dir = Path.Join(scanner.Root, dir);

		if (depth >= maxDepth || IsSubOfMatched(dir))
		{
			return new DirectoryInfo(dir)
				.EnumerateFileSystemInfos()
				.Select(Utils.ConvertFSInfo);
		}
		else if (map.TryGetValue(dir, out var files))
		{
			return files.Select(Utils.ConvertFSInfo);
		}
		else
		{
			return Enumerable.Empty<FileInformation>();
		}
	}

	bool IsSubOfMatched(string path)
	{
		var sep = new PathSpliter(path);
		while (sep.HasNext)
		{
			sep.SplitNext();
			if (matches.Contains(sep.Left))
			{
				return true;
			}
		}
		return false;
	}
}
