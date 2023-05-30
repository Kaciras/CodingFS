using System.Collections.Generic;

namespace CodingFS;

public readonly struct DetectContxt
{
	public readonly IReadOnlyList<Workspace> Parent;
	public readonly string Path;

	readonly List<Workspace> Matches;

	public DetectContxt(
		IReadOnlyList<Workspace> parent,
		string path,
		List<Workspace> matches)
	{
		Parent = parent;
		Path = path;
		Matches = matches;
	}

	public void AddWorkspace(Workspace value) => Matches.Add(value);
}

public delegate void WorkspaceFactory(DetectContxt ctx);

public interface Workspace
{
	RecognizeType Recognize(string file);
}
