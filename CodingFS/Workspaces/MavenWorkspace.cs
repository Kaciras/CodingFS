using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces;

public class MavenWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.PM;

	public static void Match(DetectContxt ctx)
	{
		if (File.Exists(Path.Join(ctx.Path, "pox.xml")))
		{
			ctx.AddWorkspace(new MavenWorkspace());
		}
	}

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
