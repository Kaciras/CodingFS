using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodingFS.Workspaces;

namespace CodingFS;

public class WorkspaceFileClassifier
{
	private readonly IEnumerable<Workspace> workspaces;

	public WorkspaceFileClassifier(IEnumerable<Workspace> workspaces)
	{
		this.workspaces = workspaces;
	}

	public FileType GetFileType(string path)
	{
		var flags = workspaces.Aggregate(RecognizeType.NotCare, (v, c) => v | c.Recognize(path));
		return GetFileType(flags);
	}

	// 【分类依据】
	// 根据 IDE 和 VCS 找出被忽略的文件，未被忽略的都是和源文件，再由项目结构的约定
	// 从被忽略的文件里区分出依赖，最后剩下的都是生成的文件。
	internal static FileType GetFileType(RecognizeType flags)
	{
		return flags.HasFlag(RecognizeType.Dependency)
			? FileType.Dependency : flags.HasFlag(RecognizeType.Ignored)
			? FileType.Generated : FileType.Source;
	}
}
