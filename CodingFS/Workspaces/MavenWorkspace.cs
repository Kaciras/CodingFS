using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces;

public sealed class MavenWorkspace : PackageManager
{
	public WorkspaceKind Kind => WorkspaceKind.PM;

	public string[] ConfigFiles => new[] { "pom.xml" };

	public PackageManager? Parent => null;

	public static void Match(DetectContxt ctx)
	{
		if (Utils.IsFile(ctx.Path, "pox.xml"))
		{
			ctx.AddWorkspace(new MavenWorkspace());
		}
	}

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
