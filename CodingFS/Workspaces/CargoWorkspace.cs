using System.IO;

namespace CodingFS.Workspaces;

public sealed class CargoWorkspace : Workspace
{
	public static void Match(DetectContxt ctx)
	{
		var toml = Path.Join(ctx.Path, "cargo.toml");
		if (File.Exists(toml))
		{
			ctx.AddWorkspace(new CargoWorkspace());
		}
	}

	public WorkspaceKind Kind => WorkspaceKind.PM;

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
