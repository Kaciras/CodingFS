using System.Collections.Generic;

namespace CodingFS;

public delegate void Detector(DetectContxt context);

public readonly struct DetectContxt
{
	public readonly IReadOnlyList<Workspace> Parent;
	public readonly string Path;

	internal readonly List<Workspace> Matches = new();

	public DetectContxt(string path, List<Workspace> parent)
	{
		Path = path;
		Parent = parent;
	}

	public void Deconstruct(
		out string path,
		out IReadOnlyList<Workspace> parent)
	{
		path = Path;
		parent = Parent;
	}

	public void AddWorkspace(Workspace value) => Matches.Add(value);
}
