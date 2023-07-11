using System;
using System.IO;

namespace CodingFS.Workspaces;

/// <summary>
/// Some files that can appear anywhere.
/// </summary>
public class CommonWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.Other;

	public RecognizeType Recognize(string path)
	{
		var name = Path.GetFileName(path.AsSpan());
		switch (name)
		{
			case "__pycache__":
			case ".pytest_cache":
			case "Thumbs.db":
				return RecognizeType.Ignored;
			case ".DS_Store":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}
}
