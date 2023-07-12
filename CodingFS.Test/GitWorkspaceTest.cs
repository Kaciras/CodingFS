using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		var i = new GitWorkspace(".");

		Assert.Equal(RecognizeType.NotCare, i.Recognize(".git"));
		Assert.Equal(RecognizeType.Ignored, i.Recognize("dist"));
	}
}
