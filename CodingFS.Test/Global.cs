using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

[module: SkipLocalsInit]

namespace CodingFS.Test;

static class Global
{
	public static void AssertNotCare(this Workspace workspace, string path)
	{
		path = Path.GetFullPath(path);
		Assert.Equal(RecognizeType.NotCare, workspace.Recognize(path));
	}

	public static void AssertDependency(this Workspace workspace, string path)
	{
		path = Path.GetFullPath(path);
		Assert.Equal(RecognizeType.Dependency, workspace.Recognize(path));
	}

	public static void AssertIgnored(this Workspace workspace, string path)
	{
		path = Path.GetFullPath(path);
		Assert.Equal(RecognizeType.Ignored, workspace.Recognize(path));
	}
}
