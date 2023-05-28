using System;
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

	int OuterDepth = 2;
	int InnerDepth = 2;

	public void Execute()
	{
		filter = new RootFileClassifier(@"D:\Coding")
		{
			OuterDepth = OuterDepth,
			InnerDepth = InnerDepth,
		};
		Execute(filter.Root, OuterDepth, InnerDepth);
	}

	private void Execute(string path, int outLimit, int innerLimit)
	{
		var info = filter.GetWorkspaces(path);
		var git = info.FindType<GitWorkspace>().FirstOrDefault();

		if (git == null)
		{
			if (--outLimit < 0)
			{
				return;
			}
		}
		else
		{
			if (--innerLimit < 0)
			{
				return;
			}
			if (info.FindType<NpmWorkspace>().Any())
			{
				CheckGit(git.Root, DateTimeOffset.Now.Add(TimeSpan.FromDays(-7)));
				return;
			}
		}
		foreach (var entry in info.ListFiles(FileType.SourceFile))
		{
			if (entry is FileInfo)
			{

			}
			else
			{
				Execute(entry.FullName, outLimit, innerLimit);
			}
		}
	}

	void CheckGit(string path, DateTimeOffset period)
	{
		using var repo = new Repository(path);
		foreach (var commit in repo.Commits)
		{
			if (commit.Committer.When < period)
			{
				Console.WriteLine($"{path} should check for update");
			}
			if (commit.Message.Contains("update deps"))
			{
				return;
			}
		}
	}
}
