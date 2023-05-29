namespace CodingFS;

public delegate Workspace? WorkspaceFactory(string path);

public interface WorkspaceDetector
{
	Workspace? Detect(string path);
}

public interface Workspace
{
	RecognizeType Recognize(string file);
}
