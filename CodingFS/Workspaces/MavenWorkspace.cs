using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces;

public class MavenWorkspace : Workspace
{
	public static Workspace? Match(List<Workspace> _, string path)
	{
		return Directory.Exists(Path.Join(path, "pox.xml")) ? new MavenWorkspace() : null;
	}

	public RecognizeType Recognize(string file) => RecognizeType.NotCare;
}
