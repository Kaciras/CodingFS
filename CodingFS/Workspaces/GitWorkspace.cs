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

	public string Folder => Path.GetDirectoryName(Repository.Info.Path[..^1])!;

 	public GitWorkspace(string path)
	{
		Repository = new Repository(path);
	}

	public RecognizeType Recognize(string path)
	{
		if (path == ".git")
		{
			return RecognizeType.NotCare;
		}
		return Ignore && Repository.Ignore.IsPathIgnored(path) 
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
