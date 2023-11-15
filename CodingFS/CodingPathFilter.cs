using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS;

public sealed class CodingPathFilter(CodingScanner scanner, FileType includes) : PathFilter
{
	readonly CodingScanner scanner = scanner;
	readonly FileType includes = includes;

	public string MapPath(string path)
	{
		return Path.Join(scanner.Root, path);
	}

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		dir = Path.Join(scanner.Root, dir);
		var w = scanner.GetWorkspaces(dir);

		return new DirectoryInfo(dir)
			.EnumerateFileSystemInfos()
			.Where(info => (w.GetFileType(info.FullName) & includes) != 0)
			.Select(Utils.ConvertFSInfo);
	}
}
