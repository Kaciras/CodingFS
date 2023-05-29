using System.Collections.Generic;

namespace CodingFS;

public delegate Workspace? WorkspaceFactory(List<Workspace> parent, string path);

public interface Workspace
{
	RecognizeType Recognize(string file);
}
