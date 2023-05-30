using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CodingFS.Workspaces;

public sealed class GitWorkspace : Workspace
{
	static bool Ignore { get; set; } = false;

	public static void Match(DetectContxt ctx)
	{
		if (Directory.Exists(Path.Join(ctx.Path, ".git")))
		{
			ctx.AddWorkspace(new GitWorkspace(ctx.Path));
		}
	}

	public Repository Repository { get; }

 	public GitWorkspace(string path)
	{
		Repository = new Repository(path);
	}

	public RecognizeType Recognize(string file)
	{
		return Ignore && Repository.Ignore.IsPathIgnored(file) 
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
