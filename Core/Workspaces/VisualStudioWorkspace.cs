using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodingFS.Helper;
using Microsoft.Build.Construction;

namespace CodingFS.Workspaces;

/// <summary>
/// Support detect project that uses VisualStudio and MSBuild, it also detects NuGet.
/// <br/>
/// Where NuGet records dependencies:
/// https://fossa.com/blog/managing-dependencies-net-csproj-packagesconfig
/// </summary>
public sealed class VisualStudioWorkspace : Workspace
{
	public string Folder { get; }

	readonly Dictionary<string, string> projects;

	public VisualStudioWorkspace(string folder, Dictionary<string, string> projects)
	{
		Folder = folder;
		this.projects = projects;
	}

	public RecognizeType Recognize(string path)
	{
		switch (PathSpliter.GetRelative(Folder, path))
		{
			case "TestResults":
			case "Debug":
			case "Release":
				return RecognizeType.Ignored;
			case ".vs":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}

	static void ParseSln(DetectContxt ctx, string file, out VisualStudioWorkspace sln)
	{
		var projects = new Dictionary<string, string>();
		var solution = SolutionFile.Parse(file);
		var hasCsharpProject = false;

		foreach (var project in solution.ProjectsInOrder)
		{
			if (!hasCsharpProject)
			{
				hasCsharpProject = project.RelativePath.EndsWith(".csproj");
			}

			var folder = Path.GetDirectoryName(project.AbsolutePath)!;
			projects[folder] = project.AbsolutePath;
		}

		if (hasCsharpProject)
		{
			ctx.AddWorkspace(new NuGetWorkspace());
		}

		ctx.AddWorkspace(sln = new VisualStudioWorkspace(ctx.Path, projects));
	}

	public static void Match(DetectContxt ctx)
	{
		var (path, parent) = ctx;
		var vsSln = parent.OfType<VisualStudioWorkspace>().FirstOrDefault();

		if (vsSln == null)
		{
			var slnFile = Directory.EnumerateFiles(path).FirstOrDefault(i => i.EndsWith(".sln"));
			if (slnFile != null)
			{
				ParseSln(ctx, slnFile, out vsSln);
			}
		}

		if (vsSln != null && vsSln.projects.TryGetValue(path, out var file))
		{
			var project = new MSBuildProject(vsSln, path, file);
			ctx.AddWorkspace(project);

			if (project.SDK == MSBuildProject.SDK_CSHARP)
			{
				var nugetRoot = parent.OfType<NuGetWorkspace>().First();
				var legacy = Utils.IsFile(path, "packages.config");
				nugetRoot.legacy |= legacy;
				ctx.AddWorkspace(new NuGetWorkspace(file, nugetRoot, legacy));
			}
		}
	}
}
