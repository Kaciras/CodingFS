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
		var workspace = new VSCodeWorkspace(".");

		workspace.AssertDependency(".vscode");
		workspace.AssertNotCare("foo");
		workspace.AssertIgnored(".idea");
		workspace.AssertNotCare("packages/.DS_Store");
	}

	[Fact]
	public void ReadSettingsWithoutExclude()
	{
		CopyTestData(Resources.vscode_empty);
		var workspace = new VSCodeWorkspace(".");
		workspace.AssertNotCare(".idea");
	}
}
