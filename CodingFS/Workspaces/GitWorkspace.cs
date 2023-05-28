using System.IO;

namespace CodingFS.Workspaces;

public sealed class GitWorkspace : Workspace
{
	public static GitWorkspace? Match(string path)
	{
		return Directory.Exists(Path.Join(path, ".git")) ? new GitWorkspace(path) : null;
	}

	public string Root { get; }

	public GitWorkspace(string root)
	{
		Root = root;
	}

	public RecognizeType Recognize(string file) => RecognizeType.NotCare;
}
