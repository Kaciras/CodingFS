using System;
using System.Collections.Generic;
using System.Text;

namespace CodingFS.Workspaces;

/// <summary>
/// 我自己的工作区有个 ThirdParty 目录放着clone的第三方项目，就识别为依赖吧。
/// </summary>
public sealed class CustomWorkspace : IWorkspace
{
	public RecognizeType Recognize(string file)
	{
		return file == @"D:\Coding\ThirdParty" ? RecognizeType.Dependency : RecognizeType.NotCare;
	}
}
