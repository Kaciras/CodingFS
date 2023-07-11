using System.IO;

namespace CodingFS.Workspaces;

public sealed class CargoWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.PM;

	public static void Match(DetectContxt ctx)
	{
		var toml = Path.Join(ctx.Path, "cargo.toml");
		if (File.Exists(toml))
		{
			ctx.AddWorkspace(new CargoWorkspace());
		}
	}

	public RecognizeType Recognize(string file) => RecognizeType.NotCare;
}
