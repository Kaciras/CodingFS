using System.IO;

namespace CodingFS.Workspaces;

/// <summary>
/// 一些很常见的文件，基本出现了都是固定的程序在使用，故不用做工作区检测而直接设为全局。
/// </summary>
public class CommonWorkspace : Workspace
{
	public RecognizeType Recognize(string path)
	{
		var name = Path.GetFileName(path);
		switch (name)
		{
			case "__pycache__":
			case "Thumbs.db":
				return RecognizeType.Ignored;
			case ".DS_Store":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}
}
