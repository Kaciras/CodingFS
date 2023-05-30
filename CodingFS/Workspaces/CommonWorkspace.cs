using System.IO;

namespace CodingFS.Workspaces;

/// <summary>
/// 一些很常见的文件，基本出现了都是固定的程序在使用，故不用做工作区检测而直接设为全局。
/// </summary>
public class CommonWorkspace : Workspace
{
	public RecognizeType Recognize(string directory)
	{
		var name = Path.GetFileName(directory);
		switch (name)
		{
			case "__pycache__":
				return RecognizeType.Ignored;
		}
		return RecognizeType.NotCare;
	}
}
