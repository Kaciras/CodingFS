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
	readonly Dictionary<string, List<string>> map = new();
	readonly HashSet<ReadOnlyMemory<char>> matches = new (Utils.memComparator);

	readonly CodingScanner scanner;
	readonly FileType includes;
	readonly int maxDepth;

	public PrebuiltPathFilter(CodingScanner scanner, FileType includes, int maxDepth)
	{
		this.scanner = scanner;
		this.includes = includes;
		this.maxDepth = maxDepth;

		BuildMap(scanner.Root, maxDepth);
	}

	bool BuildMap(string directory, int limit)
	{
		var ws = scanner.GetWorkspaces(directory);
		var files = new List<string>();

		foreach (var e in Directory.EnumerateFileSystemEntries(directory))
		{
			var type = ws.GetFileType(e);
			var matches = (type & includes) != 0;
			var subMatches = false;
			var isDir = Directory.Exists(e);

			if (type == FileType.Source && isDir && limit > 0)
			{
				subMatches = BuildMap(e, limit - 1);
			}

			if (matches && isDir)
			{
				this.matches.Add(e.AsMemory());
			}

			if (matches || subMatches)
			{
				files.Add(e);
			}
		}

		if (files.Count > 0)
		{
			map[directory] = files;
			return true;
		}
		return false;
	}

	public string MapPath(string path)
	{
		return Path.Join(scanner.Root, path);
	}

	public IEnumerable<FileInformation> ListFiles(string directory)
	{
		directory = Path.Join(scanner.Root, directory);
		var depth = directory.AsSpan().Count(Path.DirectorySeparatorChar);

		if (depth > maxDepth || IsSubOfMatched(directory))
		{
			return new DirectoryInfo(directory)
				.EnumerateFileSystemInfos()
				.Select(Utils.ConvertFSInfo);
		}
		if (map.TryGetValue(directory, out var files))
		{
			return files.Select(f =>
			{
				if (File.Exists(f))
					return Utils.ConvertFSInfo(new FileInfo(f));
				else
					return Utils.ConvertFSInfo(new DirectoryInfo(f));
			});
		}
		return Enumerable.Empty<FileInformation>();
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
