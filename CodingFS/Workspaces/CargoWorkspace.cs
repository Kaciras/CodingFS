using System.IO;

namespace CodingFS.Workspaces;

public class CargoWorkspace : Workspace
{
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
