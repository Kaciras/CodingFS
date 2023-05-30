using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingFS.Workspaces;

internal class VSCodeWorkspace : Workspace
{
	public static void Match(DetectContxt ctx)
	{
		if (Directory.Exists(Path.Join(ctx.Path, ".vscode")))
		{
			ctx.AddWorkspace(new VSCodeWorkspace(ctx.Path));
		}
	}

	public string Folder { get; }

	public VSCodeWorkspace(string folder)
	{
		Folder = folder;
	}

	public RecognizeType Recognize(string file)
	{
		var relative = Path.GetRelativePath(Folder, file);
		if (relative == ".vscode")
		{
			return RecognizeType.Dependency;
		}
		else
		{
			return RecognizeType.NotCare;
		}
	}
}
