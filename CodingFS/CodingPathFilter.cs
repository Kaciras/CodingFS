using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS;

public sealed class CodingPathFilter : PathFilter
{
	readonly CodingScanner scanner;
	readonly FileType includes;

	public CodingPathFilter(CodingScanner scanner, FileType includes)
	{
		this.scanner = scanner;
		this.includes = includes;
	}

	public CodingPathFilter(string root, FileType includes)
		: this(new CodingScanner(root), includes) { }

	public string MapPath(string path)
	{
		return Path.Join(scanner.Root, path);
	}

	public void HandleChange(string file)
	{
		file = Path.Join(scanner.Root, file);
		scanner.InvalidCache(file);
	}

	public IEnumerable<FileInformation> ListFiles(string dir)
	{
		dir = Path.Join(scanner.Root, dir);
		var ws = scanner.GetWorkspaces(dir);

		return new DirectoryInfo(dir)
			.EnumerateFileSystemInfos()
			.Where(info => ws.GetFileType(info.FullName).HasFlag(includes))
			.Select(Utils.ConvertFSInfo);
	}
}
