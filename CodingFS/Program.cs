using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using CodingFS.VFS;
using CodingFS.Workspaces;
using CommandLine;
using DokanNet;
using DokanNet.Logging;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
namespace CodingFS;

[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
internal sealed class MountArguments
{
	[Option('p', "point", Default = "x", HelpText = "指定盘符")]
	public string Point { get; set; } = "x";

	[Option('t', "type", HelpText = "要包含的文件类型")]
	public FileType Type { get; set; } = FileType.Source;
}

[Verb("inspect", HelpText = "在控制台打印出各种分类的文件")]
internal sealed class InspectArguments
{
	[Option('s', "source", HelpText = "打印源文件（可能很多）")]
	public bool Source { get; set; }
}

[Verb("clean", HelpText = "清理文件")]
internal sealed class CleanArguments
{
	[Option('b', "build", Default = true, HelpText = "删除生成的文件")]
	public bool Build { get; set; }

	[Option('d', "dep", HelpText = "删除依赖文件")]
	public bool Dependencies { get; set; }
}

internal static class Program
{
	private static readonly Workspace[] globals =
	{
		new CommonWorkspace(true),
		new CustomWorkspace(),
	};

	private static readonly WorkspaceFactory[] factories =
	{
		JetBrainsWorkspace.Match,
		NodeJSWorkspace.Match,
		GitWorkspace.Match,
		VisualStudioWorkspace.Match,
	};

	private static void Main(string[] args)
	{
		Parser.Default.ParseArguments<MountArguments, InspectArguments, CleanArguments>(args)
			.WithParsed<MountArguments>(MountVFS)
			.WithParsed<CleanArguments>(Clean)
			.WithParsed<InspectArguments>(Inspect);
	}

	private static void Inspect(InspectArguments args)
	{
		Inspect(@"D:\Coding");
	}

	private static void Inspect(string root)
	{
		var classifier = new RootFileClassifier(root, globals, factories);

		static void PrintGroup(IEnumerable<string> files, ConsoleColor color, string header)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(header);
			foreach (var file in files) Console.WriteLine(file);
			Console.ResetColor();
		}

		var groups = classifier.Group();
		PrintGroup(groups[FileType.Dependency], ConsoleColor.Blue, "Dependencies:");
		PrintGroup(groups[FileType.Generated], ConsoleColor.Red, "Generated files:");
	}

	private static void Clean(CleanArguments args)
	{
		Clean(@"D:\Coding", args);
	}

	private static void Clean(string root, CleanArguments args)
	{
		var classifier = new RootFileClassifier(root, globals, factories);
		var countDeps = 0;
		var countBuild = 0;

		static void Delete(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			else
			{
				Directory.Delete(path, true);
			}
		}

		foreach (var (file, type) in classifier.Iterate())
		{
			switch (type)
			{
				case FileType.Dependency
				when args.Dependencies:
					Delete(file);
					countDeps++;
					break;
				case FileType.Generated
				when args.Build:
					Delete(file);
					countBuild++;
					break;
			}
		}
		Console.WriteLine($"清理完毕，删除了{countBuild}个生成的文件/目录，和{countDeps}个依赖文件/目录");
	}

	private static void MountVFS(MountArguments args)
	{
		var map = new Dictionary<string, RootFileClassifier>
		{
			["Coding"] = new RootFileClassifier(@"D:\Coding", globals, factories),
		};

#if DEBUG
		using var dokanLogger = new ConsoleLogger("[Dokan] ");
		var mountOptions = DokanOptions.DebugMode | DokanOptions.StderrOutput;
#else
		var mountOptions = default(DokanOptions);
		var dokanLogger = new NullLogger();
		Console.WriteLine($@"CodingFS mounted at x:\");
#endif

		using var dokan = new Dokan(dokanLogger);
		using var instance = new DokanInstanceBuilder(dokan)
			.ConfigureOptions(options =>
			{
				options.MountPoint = "x:\\";
				options.Options = mountOptions;
			})
			.Build(AopFSWrapper.Create(new UnsafeCodingFS(args.Type, map)));

		new ManualResetEvent(false).WaitOne();
	}
}
