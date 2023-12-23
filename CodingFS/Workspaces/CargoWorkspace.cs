using System.Linq;
using CodingFS.Helper;

namespace CodingFS.Workspaces;

public sealed class CargoWorkspace : PackageManager
{
	static readonly string[] ROOT_FILES = ["cargo.toml", "cargo.lock"];
	static readonly string[] MODULE_FILES = ["cargo.toml"];

	public static void Match(DetectContxt ctx)
	{
		if (!Utils.IsFile(ctx.Path, "cargo.toml"))
		{
			return;
		}

		var parent = Utils.IsFile(ctx.Path, "cargo.lock")
			? null
			: ctx.Parent.OfType<CargoWorkspace>().FirstOrDefault();

		ctx.AddWorkspace(new CargoWorkspace(parent));
	}

	public string[] ConfigFiles => Root == this ? ROOT_FILES : MODULE_FILES;

	public PackageManager Root { get; }

	public CargoWorkspace(CargoWorkspace? root)
	{
		Root = root ?? this;
	}

	public RecognizeType Recognize(string path) => RecognizeType.NotCare;
}
