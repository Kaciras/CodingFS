using CodingFS.Helper;

namespace CodingFS.Workspaces;

public sealed class MavenWorkspace : PackageManager
{
	public string[] ConfigFiles => ["pom.xml"];

	public PackageManager Root => this;

	public static void Match(DetectContxt ctx)
	{
		if (Utils.IsFile(ctx.Path, "pox.xml"))
		{
			ctx.AddWorkspace(new MavenWorkspace());
		}
	}

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
