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
	static readonly string SEP = Path.DirectorySeparatorChar.ToString();

	readonly Dictionary<string, PathFilter> filters = new();
	readonly DateTime creation = DateTime.Now;

	public void Set(string path, PathFilter filter)
	{
		var name = Path.GetFileName(path.AsSpan());
		if (name.Length == path.Length)
		{
			filters[path] = filter;
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
		if (dir == SEP)
		{
			return filters.Keys.Select(x => new FileInformation
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

	PathFilter Get(string value, out string relative)
	{
		var split = value.Split(Path.DirectorySeparatorChar, 3);
		if (filters.TryGetValue(split[1], out var filter))
		{
			if (split.Length < 3)
			{
				relative = "";
			}
			else
			{
				relative = split[2];
			}
			return filter;
		}
		throw new FileNotFoundException("Path is not in the map", value);
	}
}
