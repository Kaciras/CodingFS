using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CodingFS.Workspaces;
using CommandLine;
using LibGit2Sharp;

namespace CodingFS.Command;

[Verb("keep-green", HelpText = "Keep dependencies update-to-date")]
internal sealed class KeepGreen : CliCommand
{
	private RootFileClassifier filter;

	int OuterDepth = 3;
	int InnerDepth = 3;

	public void Execute()
	{
		filter = new RootFileClassifier(@"D:\Coding")
		{
			OuterDepth = OuterDepth,
			InnerDepth = InnerDepth,
		};
		IterateProject(filter.Root, OuterDepth);
	}

	TimeSpan UpdateCycle(Workspace workspace) => workspace switch
	{
		NpmWorkspace => TimeSpan.FromDays(7),
		MavenWorkspace => TimeSpan.FromDays(60),
		CargoWorkspace => TimeSpan.FromDays(30),
		MSBuildProject w when
		(w.SDK == MSBuildProject.SDK_CSHARP) => TimeSpan.FromDays(60),
		_ => TimeSpan.MaxValue,
	};

	void IterateProject(string path, int outLimit)
	{
		var info = filter.GetWorkspaces(path);
		var git = info.FindType<GitWorkspace>().FirstOrDefault();

		if (git != null)
		{
			//IterateSubmodule(path, InnerDepth);
			GitGC(git.Repository);
		}

		if (--outLimit == 0)
		{
			return;
		}

		foreach (var entry in info.ListFiles(FileType.SourceFile))
		{
			if (entry is DirectoryInfo)
			{
				IterateProject(entry.FullName, outLimit);
			}
		}
	}

	void IterateSubmodule(string path, int innerLimit)
	{

	}


	void CheckGit(Repository repo, DateTimeOffset period)
	{
		var project = Path.GetDirectoryName(repo.Info.Path);
		foreach (var commit in repo.Commits)
		{
			if (commit.Committer.When < period)
			{
				Console.WriteLine($"{project} should check for update");
			}
			if (commit.Message.Contains("update deps"))
			{
				return;
			}
		}
	}

	void GitGC(Repository repo)
	{
		var project = Path.GetDirectoryName(repo.Info.Path[..^1]);

		Process.Start(new ProcessStartInfo()
		{
			FileName = "git",
			Arguments = "reflog expire --all --expire=now",
			WorkingDirectory = project,
		})!.WaitForExit();

		Process.Start(new ProcessStartInfo()
		{
			FileName = "git",
			Arguments = "gc --aggressive --prune=now --quiet",
			WorkingDirectory = project,
		})!.WaitForExit();

		Console.WriteLine($"GC completed on {project}");
	}
}
