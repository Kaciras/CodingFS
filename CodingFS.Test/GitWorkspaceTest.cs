using System;
using System.IO;
using CodingFS.Workspaces;
using LibGit2Sharp;
using Xunit;

namespace CodingFS.Test;

public sealed class GitWorkspaceTest
{
	[Fact]
	public void ReadPatternsFromGitignore()
	{
		Repository.Init(".");
		File.WriteAllText(".gitignore", "/dist");

		GitWorkspace.Ignore = true;
		var i = new GitWorkspace(Environment.CurrentDirectory);

		Assert.Equal(RecognizeType.NotCare, i.Recognize(Path.GetFullPath(".git")));
		Assert.Equal(RecognizeType.Ignored, i.Recognize(Path.GetFullPath("dist")));
	}
}
