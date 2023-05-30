using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace CodingFS.Workspaces;

public class VisualStudioWorkspace : Workspace
{
	public string Folder { get; }

	internal readonly Dictionary<string, string> projects;

	public VisualStudioWorkspace(string folder, Dictionary<string, string> projects)
	{
		Folder = folder;
		this.projects = projects;
	}

	public RecognizeType Recognize(string file)
	{
		switch (Path.GetRelativePath(Folder, file))
		{
			case "TestResults":
			case "Debug":
			case "Release":
				return RecognizeType.Ignored;
			case ".vs":
			case "packages":
				return RecognizeType.Dependency;
			default:
				return RecognizeType.NotCare;
		}
	}

	public static void Match(DetectContxt ctx)
	{
		var vsSln = ctx.Parent.OfType<VisualStudioWorkspace>().FirstOrDefault();

		if (vsSln == null)
		{
			var slnFile = Directory.EnumerateFiles(ctx.Path)
				.FirstOrDefault(i => i.EndsWith(".sln"));

			if (slnFile != null)
			{
				var projects = new Dictionary<string, string>();
				var solution = SolutionFile.Parse(slnFile);

				foreach (var project in solution.ProjectsInOrder)
				{
					var type = Path.GetExtension(project.RelativePath);
					var folder = Path.GetDirectoryName(project.AbsolutePath)!;

					projects[folder] = project.AbsolutePath;
				}

				vsSln = new VisualStudioWorkspace(ctx.Path, projects);
				ctx.AddWorkspace(vsSln);
			}
		}

		if (vsSln != null && vsSln.projects.TryGetValue(ctx.Path, out var file))
		{
			ctx.AddWorkspace(new MSBuildProject(ctx.Path, file));
		}
	}
}

public class MSBuildProject : Workspace
{
	public static readonly string[] SDK_CSHARP = { "obj" , "bin" };
	public static readonly string[] SDK_CPP = { "Debug", "Release", "x64", "win32" };

	public string Folder { get; }

	public string[] SDK { get; }

	public MSBuildProject(string folder, string file)
	{
		Folder = folder;
		SDK = Path.GetExtension(file) switch
		{
			".csproj" => SDK_CSHARP,
			".vcxproj" => SDK_CPP,
			_ => throw new NotImplementedException(),
		};
	}

	public RecognizeType Recognize(string file)
	{
		return SDK.Contains(Path.GetRelativePath(Folder, file)) 
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
