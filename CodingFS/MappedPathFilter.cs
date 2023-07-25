using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS;

/// <summary>
/// A vitrual directory in file system, 
/// </summary>
public sealed class MappedPathFilter : PathFilter
{
	static readonly char SEP = Path.DirectorySeparatorChar;

	readonly Dictionary<ReadOnlyMemory<char>, PathFilter> filters = new(Utils.memComparator);
	readonly List<string> directories = new();

	readonly DateTime creation = DateTime.Now;

	public void Set(string path, PathFilter filter)
	{
		if (path.AsSpan().IndexOfAny('\\', '/') == -1)
		{
			filters[path.AsMemory()] = filter;
			directories.Add(path);
		}
		else
		{
			throw new ArgumentException("Subfolder is not supported");
		}
	}

	public void HandleChange(string file)
	{
		Get(file, out var relative).HandleChange(relative);
	}

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		if (dir.Length == 1 && dir[0] == SEP)
		{
			return directories.Select(x => new FileInformation
			{
				CreationTime = creation,
				FileName = x,
				Attributes = FileAttributes.Directory,
			});
		}
		return Get(dir, out var relative).ListFiles(relative);
	}

	public string MapPath(string path)
	{
		return Get(path, out var relative).MapPath(relative);
	}

	PathFilter Get(string input, out string relative)
	{
		var splitor = new PathSpliter(input);

		// Skip the root slash.
		switch (input[0])
		{
			case '\\':
			case '/':
				splitor.Index = 0;
				break;
		}

		var top = splitor.SplitNext();
		if (filters.TryGetValue(top, out var filter))
		{
			if (splitor.HasNext)
			{
				relative = new string(splitor.Right.Span);
			}
			else
			{
				relative = string.Empty;
			}
			return filter;
		}
		throw new FileNotFoundException("Path is not in the map", input);
	}
}
