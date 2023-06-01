using System;
using System.Collections;
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

	void IterateProject(string path, int outLimit)
	{
		var info = filter.GetWorkspaces(path);
		var git = info.FindType<GitWorkspace>().FirstOrDefault();

		if (git != null)
		{
			CheckUpdatable(git);
			//GitGC(git.Repository);
			return;
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

	TimeSpan MinUpdateCycle(string path, int innerLimit)
	{
		var info = filter.GetWorkspaces(path);
		var time = TimeSpan.MaxValue;

		void SetTime(TimeSpan value)
		{
			if (value < time) time = value;
		}

		foreach (var workspace in info.Current)
		{
			switch (workspace)
			{
				case NpmWorkspace:
					SetTime(TimeSpan.FromDays(7));
					break;
				case MavenWorkspace:
					SetTime(TimeSpan.FromDays(90));
					break;
				case CargoWorkspace:
					SetTime(TimeSpan.FromDays(30));
					break;
				case MSBuildProject w
				when (w.SDK == MSBuildProject.SDK_CSHARP):
					SetTime(TimeSpan.FromDays(90));
					break;
			}
		}

		if (--innerLimit > 0)
		{
			foreach (var entry in info.ListFiles(FileType.SourceFile))
			{
				if (entry is DirectoryInfo)
				{
					SetTime(MinUpdateCycle(entry.FullName, innerLimit));
				}
			}
		}

		return time;
	}

	void CheckUpdatable(GitWorkspace git)
	{
		var period = MinUpdateCycle(git.Folder, InnerDepth);
		if (period == TimeSpan.MaxValue)
		{
			return;
		}

		var project = Path.GetFileName(git.Folder);
		var now = DateTimeOffset.Now;
		try
		{
			foreach (var commit in git.Repository.Commits)
			{
				var duration = now - commit.Committer.When;
				if (duration > period)
				{
					Console.WriteLine($"{project} should check for update ({duration.Days} days)");
					break;
				}
				if (commit.Message.Contains("update deps"))
				{
					break;
				}
			}
		} 
		catch (NotFoundException)
		{
			// https://github.com/GitTools/GitVersion/issues/1043
		}
	}

	void GitGC(GitWorkspace git)
	{
		Process.Start(new ProcessStartInfo("git")
		{
			Arguments = "reflog expire --all --expire=now",
			WorkingDirectory = git.Folder,
		})!.WaitForExit();

		Process.Start(new ProcessStartInfo("git")
		{
			Arguments = "gc --aggressive --prune=now --quiet",
			WorkingDirectory = git.Folder,
		})!.WaitForExit();

		Console.WriteLine($"GC completed on {git.Folder}");
	}
}
