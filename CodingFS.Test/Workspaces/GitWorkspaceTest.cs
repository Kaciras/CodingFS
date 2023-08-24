using System;
using System.IO;
using CodingFS.Workspaces;
using LibGit2Sharp;
using Xunit;

namespace CodingFS.Test.Workspaces;

public sealed class GitWorkspaceTest
{
	[Fact]
	public void ReadPatternsFromGitignore()
	{
		Repository.Init(".");
		File.WriteAllText(".gitignore", "/dist");

		var workspace = new GitWorkspace(Environment.CurrentDirectory, RecognizeType.Ignored);

		workspace.AssertNotCare(".git");
		workspace.AssertIgnored("dist");
	}
}
