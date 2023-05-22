using System.IO;

namespace CodingFS.Workspaces;

public sealed class GitWorkspace : Workspace
{
	public static GitWorkspace? Match(string path)
	{
		return Directory.Exists(Path.Join(path, ".git")) ? new GitWorkspace() : null;
	}

	public RecognizeType Recognize(string file) => RecognizeType.NotCare;
}
