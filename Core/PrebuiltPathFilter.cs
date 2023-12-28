using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodingFS.Helper;
using DokanNet;

namespace CodingFS;

///  <summary>
/// This class searches for all visible files at startup, and it solves the problem that 
/// CodingPathFilter could not display deep files when Source was not included in the includes parameter.
/// <br/>
/// For example, consider there is a folder A with its sub folder B, the type of A is Source and
/// the type of B is Generated. In order to view only the generated files, you can use the mount command
/// with the parameter `--type=Generated`, but no files will be displayed because the type of A is not Generated. 
/// <br/>
/// The correct result is to show the directory A, and the files within it B. To fix this we need to 
/// traverse the directory tree to find directories which contains matched files.
/// </summary>
public class PrebuiltPathFilter : PathFilter
{
	/// <summary>
	/// Directory path with its files to show (it or it has subfiles matching the includes).
	/// </summary>
	readonly Dictionary<string, List<FileSystemInfo>> map = new();

	/// <summary>
	/// Path of files matching the includes parameter.
	/// </summary>
	readonly HashSet<ReadOnlyMemory<char>> matches = new (Utils.memComparator);

	readonly CodingScanner scanner;
	readonly FileType includes;

	public PrebuiltPathFilter(CodingScanner scanner, FileType includes)
	{
		this.scanner = scanner;
		this.includes = includes;

		BuildMap(new DirectoryInfo(scanner.Root), scanner.MaxDepth - 1);
	}

	void BuildMap(DirectoryInfo directory, int limit)
	{
		var ws = scanner.GetWorkspaces(directory.FullName);
		var visibleFiles = new List<FileSystemInfo>();

		foreach (var e in directory.EnumerateFileSystemInfos())
		{
			var type = ws.GetFileType(e.FullName);
			var included = (type & includes) != 0;
			var sizeBefore = map.Count;

			if (type == FileType.Source
				&& e is DirectoryInfo next
				&& limit > 0)
			{
				BuildMap(next, limit - 1);
			}

			if (included || map.Count > sizeBefore)
			{
				visibleFiles.Add(e);
			}
			if (included && e is DirectoryInfo)
			{
				matches.Add(e.FullName.AsMemory());
			}
		}

		if (visibleFiles.Count > 0)
		{
			map[directory.FullName] = visibleFiles;
		}
	}

	public string MapPath(string path)
	{
		return Path.Join(scanner.Root, path);
	}

	bool AncestorMatches(string path)
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

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		var depth = dir.AsSpan().Count(Path.DirectorySeparatorChar);
		dir = Path.Join(scanner.Root, dir);

		// The root is not included here, so we need to -1.
		var maxDepth = scanner.MaxDepth - 1;

		if (depth >= maxDepth || AncestorMatches(dir))
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
}
