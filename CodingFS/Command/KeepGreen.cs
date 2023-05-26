using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingFS.Workspaces;
using CommandLine.Text;
using CommandLine;

namespace CodingFS.Command;


[Verb("keep-green", HelpText = "Keep dependencies update-to-date")]
internal sealed class KeepGreen : CliCommand
{
	public void Execute()
	{
		var filter = new RootFileClassifier(@"D:\Coding")
		{
			OuterDepth = 3,
			InnerDepth = 2,
		};
		Execute(filter, filter.Root);
	}

	private void Execute(RootFileClassifier filter, string path)
	{
		var info = filter.GetWorkspaces(path);
		if (info.FindType<NpmWorkspace>().Any())
		{

		}
		foreach (var entry in info.ListFiles(FileType.SourceFile))
		{
			if (entry is FileInfo)
			{

			}
			else
			{
				Execute(filter, entry.FullName);
			}
		}
	}
}
