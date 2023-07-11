using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingFS.Workspaces;
using Xunit;

namespace CodingFS.Test;

public class VSCodeWorkspaceTest
{
	[Fact]
	public void Test()
	{
		var i = new VSCodeWorkspace("Resources");
		Assert.Equal(RecognizeType.Dependency, i.Recognize(".vscode"));
		Assert.Equal(RecognizeType.NotCare, i.Recognize("foo"));
		Assert.Equal(RecognizeType.Ignored, i.Recognize(".idea"));
		Assert.Equal(RecognizeType.NotCare, i.Recognize("packages/.DS_Store"));
	}
}
