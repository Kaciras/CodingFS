using System.IO;
using LibGit2Sharp;

namespace CodingFS.Workspaces;

public sealed class GitWorkspace : Workspace
{
	public static bool Ignore { get; set; } = false;

	public static void Match(DetectContxt ctx)
	{
		if (Utils.IsDir(ctx.Path, ".git"))
		{
			ctx.AddWorkspace(new GitWorkspace(ctx.Path));
		}
	}

	public WorkspaceKind Kind => WorkspaceKind.VCS;

	// Native handles in Repository implements destruction function,
	// so just let GC to dispose them.
	public Repository Repository { get; }

	public string Folder { get; }

 	public GitWorkspace(string path)
	{
		Folder = path;
		Repository = new Repository(path);
	}

	public RecognizeType Recognize(string path)
	{
		if (!Ignore)
		{
			return RecognizeType.NotCare;
		}
		var relative = Path.GetRelativePath(Folder, path);
		return Repository.Ignore.IsPathIgnored(relative)
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
