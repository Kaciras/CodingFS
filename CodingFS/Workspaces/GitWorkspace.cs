using System;
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
		var relative = PathSpliter.GetRelative(Folder, path);
		if (relative.SequenceEqual(".git"))
		{
			// .git is default ignored by IsPathIgnored().
			return RecognizeType.NotCare;
		}
		if (!Ignore)
		{
			return RecognizeType.NotCare;
		}
		return Repository.Ignore.IsPathIgnored(new string(relative))
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
