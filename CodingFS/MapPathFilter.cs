using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS;

public sealed class MapPathFilter : PathFilter
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
		file = GetPath(file, out var filter);
		filter.HandleChange(file);
	}

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		if (dir != SEP)
		{
			dir = GetPath(dir, out var filter);
			return filter.ListFiles(dir);
		}
		return filters.Keys.Select(x => new FileInformation
		{
			CreationTime = creation,
			FileName = x,
			Attributes = FileAttributes.Directory,
		});
	}

	public string MapPath(string path)
	{
		path = GetPath(path, out var filter);
		return filter.MapPath(path);
	}

	string GetPath(string value, out PathFilter filter)
	{
		var split = value.Split(Path.DirectorySeparatorChar, 3);
		if (filters.TryGetValue(split[1], out filter!))
		{
			if (split.Length < 3)
			{
				return "";
			}
			return split[2];
		}
		throw new FileNotFoundException("Path is not in the map", value);
	}
}
