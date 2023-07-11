using System;
using System.Collections.Generic;
using System.Text;

namespace CodingFS.Workspaces;

/// <summary>
/// 我自己的工作区有个 ThirdParty 目录放着clone的第三方项目，就识别为依赖吧。
/// </summary>
public sealed class CustomWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.PM;

	public RecognizeType Recognize(string path)
	{
		switch (path)
		{
			case @"ThirdParty":
			case @"Blog\data":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}
}
