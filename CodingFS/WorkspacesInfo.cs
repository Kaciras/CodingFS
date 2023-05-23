using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodingFS;

public sealed class WorkspacesInfo
{
	private readonly IList<Workspace> workspaces;
	private readonly string path;

	internal WorkspacesInfo(string path, IList<Workspace> workspaces)
	{
		this.path = path;
		this.workspaces = workspaces;
	}

	public IEnumerable<T> FindType<T>() where T : Workspace
	{
		return workspaces.OfType<T>();
	}

	public FileType GetFileType(string path)
	{
		var flags = workspaces.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path));
		return GetFileType(flags);
	}

	// 【分类依据】
	// 根据 IDE 和 VCS 找出被忽略的文件，未被忽略的都是和源文件，再由项目结构的约定
	// 从被忽略的文件里区分出依赖，最后剩下的都是生成的文件。
	static FileType GetFileType(RecognizeType flags)
	{
		return flags.HasFlag(RecognizeType.Dependency)
			? FileType.Dependency : flags.HasFlag(RecognizeType.Ignored)
			? FileType.Generated : FileType.Source;
	}

	public IEnumerable<FileSystemInfo> ListFiles(FileType type)
	{
		return new DirectoryInfo(path)
			.EnumerateFileSystemInfos()
			.Where(info => GetFileType(info.FullName).HasFlag(type));
	}
}
