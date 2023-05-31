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
		var flags = Workspaces.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path));
		return GetFileType(flags);
	}

	/// <summary>
	/// Judging the file type by the classification result of workspaces.
	/// </summary>
	static FileType GetFileType(RecognizeType flags)
	{
		if (flags.HasFlag(RecognizeType.Dependency))
		{
			return FileType.Dependency;
		}
		else if (flags.HasFlag(RecognizeType.Ignored))
		{
			return FileType.Generated;
		}
		else
		{
			return FileType.SourceFile;
		}
	}

	public IEnumerable<FileSystemInfo> ListFiles(FileType type)
	{
		return new DirectoryInfo(path)
			.EnumerateFileSystemInfos()
			.Where(info => GetFileType(info.FullName).HasFlag(type));
	}
}
