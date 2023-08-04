using System.IO;
using CodingFS.Test.Properties;
using CodingFS.Workspaces;
using Xunit;

namespace CodingFS.Test.Workspaces;

public sealed class VSCodeWorkspaceTest
{
	static void CopyTestData(byte[] settings)
	{
		Directory.CreateDirectory(".vscode");
		File.WriteAllBytes(".vscode/settings.json", settings);
	}

	[Fact]
	public void ReadExcludes()
	{
		CopyTestData(Resources.vscode_exclude);
		var i = new VSCodeWorkspace(".");
		Assert.Equal(RecognizeType.Dependency, i.Recognize(".vscode"));
		Assert.Equal(RecognizeType.NotCare, i.Recognize("foo"));
		Assert.Equal(RecognizeType.Ignored, i.Recognize(".idea"));
		Assert.Equal(RecognizeType.NotCare, i.Recognize("packages/.DS_Store"));
	}

	[Fact]
	public void ReadSettingsWithoutExclude()
	{
		CopyTestData(Resources.vscode_empty);
		var i = new VSCodeWorkspace(".");
		Assert.Equal(RecognizeType.NotCare, i.Recognize(".idea"));
	}
}
