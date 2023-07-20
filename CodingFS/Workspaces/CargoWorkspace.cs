using System.IO;
using System.Linq;

namespace CodingFS.Workspaces;

public sealed class CargoWorkspace : PackageManager
{
	static readonly string[] ROOT_FILES = { "cargo.toml", "cargo.lock" };
	static readonly string[] MODULE_FILES = { "cargo.toml" };

	public static void Match(DetectContxt ctx)
	{
		var tomlFile = Path.Join(ctx.Path, "cargo.toml");
		var lockFile = Path.Join(ctx.Path, "cargo.lock");

		if (!File.Exists(tomlFile))
		{
			return;
		}

		var parent = File.Exists(lockFile)
			? null
			: ctx.Parent.OfType<CargoWorkspace>().FirstOrDefault();

		ctx.AddWorkspace(new CargoWorkspace(parent));
	}

	public WorkspaceKind Kind => WorkspaceKind.PM;

	public string[] ConfigFiles => Parent == null ? ROOT_FILES : MODULE_FILES;

	public PackageManager? Parent { get; }

	public CargoWorkspace(CargoWorkspace? parent)
	{
		Parent = parent;
	}

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
