using System.Collections.Generic;
using CodingFS.Workspaces;

namespace CodingFS;

public delegate void Detector(DetectContxt context);

public class BuiltinDetectorOptions
{
	public RecognizeType Gitignore { get; set; }
}

public static class Detectors
{
	public static Detector[] GetBuiltins(BuiltinDetectorOptions options)
	{
		return [
			new JetBrainsDetector().Detect,
			new GitDetector(options.Gitignore).Match,
			NpmWorkspace.Match,
			MavenWorkspace.Match,
			CargoWorkspace.Match,
			VSCodeWorkspace.Match,
			VisualStudioWorkspace.Match,
		];
	}
}

public readonly struct DetectContxt(string path, List<Workspace> parent)
{
	public readonly IReadOnlyList<Workspace> Parent = parent;
	public readonly string Path = path;

	internal readonly List<Workspace> Matches = [];

	public void Deconstruct(out string path, out IReadOnlyList<Workspace> parent)
	{
		path = Path;
		parent = Parent;
	}

	public void AddWorkspace(Workspace value) => Matches.Add(value);
}
