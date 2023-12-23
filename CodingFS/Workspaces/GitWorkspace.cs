using System;
using CodingFS.Helper;
using LibGit2Sharp;

namespace CodingFS.Workspaces;

public sealed class GitDetector
{
	/// <summary>
	/// RecognizeType of files matched by .gitignore, default is NotCare.
	/// </summary>
	readonly RecognizeType ignored;

	public GitDetector(RecognizeType ignored)
	{
		this.ignored = ignored;
	}

	public void Match(DetectContxt ctx)
	{
		if (Utils.IsDir(ctx.Path, ".git"))
		{
			ctx.AddWorkspace(new GitWorkspace(ctx.Path, ignored));
		}
	}
}

public sealed class GitWorkspace : Workspace
{
	// Native handles in Repository implements destruction function,
	// so just let GC to dispose them.
	public Repository Repository { get; }

	public string Folder { get; }

	readonly RecognizeType ignored;

	public GitWorkspace(string path, RecognizeType ignored)
	{
		this.ignored = ignored;
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
		if (ignored == RecognizeType.NotCare)
		{
			return RecognizeType.NotCare;
		}
		return Repository.Ignore.IsPathIgnored(new string(relative))
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
