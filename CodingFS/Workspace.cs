namespace CodingFS;

public delegate Workspace? WorkspaceFactory(string path);

public interface Workspace
{
	RecognizeType Recognize(string file);
}
