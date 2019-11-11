using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CodingFS.Workspaces
{
	public class NodeJSWorkspaceFactory : IWorkspaceFactory
	{
		public IWorkspace? Match(string path)
		{
			if(File.Exists(Path.Combine(path, "package.json")))
			{
				return new NodeJSWorkspace(path);
			}
			return null;
		}
	}

	public class NodeJSWorkspace : IWorkspace
	{
		readonly string root;

		public NodeJSWorkspace(string root)
		{
			this.root = root;
		}

		public RecognizeType Recognize(string path)
		{
			if (Path.GetFileName(path) == "node_modules")
			{
				return RecognizeType.Dependency;
			}
			if (Path.GetRelativePath(root, path) == "dist")
			{
				return RecognizeType.Ignored;
			}
			return RecognizeType.NotCare;
		}
	}
}
