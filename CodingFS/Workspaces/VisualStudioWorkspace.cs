using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace CodingFS.Workspaces;

/// <summary>
/// Support detect project that uses VisualStudio and MSBuild, it also detects NuGet.
/// <br/>
/// Where NuGet records dependencies:
/// https://fossa.com/blog/managing-dependencies-net-csproj-packagesconfig
/// </summary>
public class VisualStudioWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.IDE;

	public string Folder { get; }

	readonly Dictionary<string, string> projects;

	bool legacyNuGet;

	public VisualStudioWorkspace(string folder, Dictionary<string, string> projects)
	{
		Folder = folder;
		this.projects = projects;
	}

	public RecognizeType Recognize(string path)
	{
		switch (path)
		{
			case "TestResults":
			case "Debug":
			case "Release":
				return RecognizeType.Ignored;
			case ".vs":
			case "packages" when legacyNuGet:
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}

	static VisualStudioWorkspace ParseSln(string dir, string file)
	{
		var projects = new Dictionary<string, string>();
		var solution = SolutionFile.Parse(file);

		foreach (var project in solution.ProjectsInOrder)
		{
			var type = Path.GetExtension(project.RelativePath);
			var folder = Path.GetDirectoryName(project.AbsolutePath)!;

			projects[folder] = project.AbsolutePath;
		}

		return new VisualStudioWorkspace(dir, projects);
	}

	public static void Match(DetectContxt ctx)
	{
		var (path, parent) = ctx;
		var vsSln = parent.OfType<VisualStudioWorkspace>().FirstOrDefault();

		if (vsSln == null)
		{
			var slnFile = Directory.EnumerateFiles(path)
				.FirstOrDefault(i => i.EndsWith(".sln"));

			if (slnFile != null)
			{
				vsSln = ParseSln(path, slnFile);
				ctx.AddWorkspace(vsSln);
			}
		}

		if (vsSln != null && vsSln.projects.TryGetValue(path, out var file))
		{
			var project = new MSBuildProject(vsSln, path, file);
			ctx.AddWorkspace(project);

			if (project.SDK == MSBuildProject.SDK_CSHARP)
			{
				vsSln.legacyNuGet = vsSln.legacyNuGet || Path.Exists(path);
			}
		}
	}
}
