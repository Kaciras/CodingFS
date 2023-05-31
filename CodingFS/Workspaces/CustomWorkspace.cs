using System;
using System.Collections.Generic;
using System.Text;

namespace CodingFS.Workspaces;

/// <summary>
/// 我自己的工作区有个 ThirdParty 目录放着clone的第三方项目，就识别为依赖吧。
/// </summary>
public sealed class CustomWorkspace : Workspace
{
	public RecognizeType Recognize(string file)
	{
		switch (file)
		{
			case @"D:\Coding\ThirdParty":
			case @"D:\Coding\Blog\data":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}
}
