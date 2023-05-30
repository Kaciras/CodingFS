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

	public static Workspace? Match(List<Workspace> ancestor, string path)
	{
		var vsSln = ancestor.OfType<VisualStudioWorkspace>().FirstOrDefault();
		if (vsSln != null)
		{
			if (vsSln.projects.TryGetValue(path, out var file))
			{
				return new MSBuildProject(path, file);
			}
			return null;
		}
		else
		{
			var sln = Directory.EnumerateFiles(path).FirstOrDefault(i => i.EndsWith(".sln"));
			if (sln == null)
			{
				return null;
			}

			var projects = new Dictionary<string, string>();
			var solution = SolutionFile.Parse(sln);

			foreach (var project in solution.ProjectsInOrder)
			{
				var type = Path.GetExtension(project.RelativePath);
				var folder = Path.GetDirectoryName(project.AbsolutePath)!;

				projects[folder] = project.AbsolutePath;
			}

			return new VisualStudioWorkspace(path, projects);
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
