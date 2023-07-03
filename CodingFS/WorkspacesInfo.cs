using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodingFS;

public sealed class WorkspacesInfo
{
	public IReadOnlyList<Workspace> Workspaces { get; }

	public IReadOnlyList<Workspace> Current { get; }

	private readonly string path;

	internal WorkspacesInfo(string path, IReadOnlyList<Workspace> workspaces, IReadOnlyList<Workspace> current)
	{
		this.path = path;
		Workspaces = workspaces;
		Current = current;
	}

	public IEnumerable<T> FindType<T>() where T : Workspace
	{
		return Workspaces.OfType<T>();
	}

	public FileType GetFileType(string path)
	{
		return Workspaces
			.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path))
			.ToFileType();
	}

	public IEnumerable<FileSystemInfo> ListFiles(FileType type)
	{
		return new DirectoryInfo(path)
			.EnumerateFileSystemInfos()
			.Where(info => GetFileType(info.FullName).HasFlag(type));
	}
}
