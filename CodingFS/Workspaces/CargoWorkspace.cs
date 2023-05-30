using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingFS.Workspaces;

public class CargoWorkspace : Workspace
{
	public static Workspace? Match(List<Workspace> _, string path)
	{
		return Directory.Exists(Path.Join(path, "cargo.toml")) ? new CargoWorkspace() : null;
	}

	public RecognizeType Recognize(string file) => RecognizeType.NotCare;
}
