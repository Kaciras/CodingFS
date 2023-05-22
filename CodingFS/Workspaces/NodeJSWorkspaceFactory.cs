using System.IO;

namespace CodingFS.Workspaces;

public class NodeJSWorkspace : Workspace
{
	public string PackageManager { get; }

	readonly string root;

	public NodeJSWorkspace(string root, string packageManager)
	{
		this.root = root;
		PackageManager = packageManager;
	}

	public RecognizeType Recognize(string path)
	{
		if (Path.GetFileName(path) == "node_modules")
		{
			return RecognizeType.Dependency;
		}
		//if (Path.GetRelativePath(root, path) == "dist")
		//{
		//	return RecognizeType.Ignored;
		//}
		return RecognizeType.NotCare;
	}

	public static Workspace? Match(string path)
	{
		if (!File.Exists(Path.Combine(path, "package.json")))
		{
			return null;
		}

		var packageManager = "npm";
		if (!File.Exists(Path.Combine(path, "pnpm-lock.yaml")))
		{
			packageManager = "pnpm";
		}

		return new NodeJSWorkspace(path, packageManager);
	}
}
