using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using CodingFS.VirtualFileSystem;
using CodingFS.Workspaces;
using CommandLine;
using DokanNet;
using DokanNet.Logging;

[assembly: InternalsVisibleTo("CodingFS.Test")]
[assembly: InternalsVisibleTo("CodingFS.Benchmark")]
namespace CodingFS
{
	[Verb("mount", HelpText = "挂载为虚拟磁盘，仅包含指定类型的文件")]
	internal sealed class MountOptions
	{
		[Option('p', "point", Default = "x", HelpText = "指定盘符")]
		public string Point { get; set; } = "x";

		[Option('t', "type", HelpText = "要包含的文件类型")]
		public FileType Type { get; set; } = FileType.Source;
	}

	[Verb("inspect", HelpText = "在控制台打印出各种分类的文件")]
	internal sealed class InspectOptions
	{
		[Option('s', "source", HelpText = "打印源文件（可能很多）")]
		public bool Source { get; set; }
	}

	[Verb("clean", HelpText = "清理文件")]
	internal sealed class CleanOptions
	{
		[Option('b', "build", Default = true, HelpText = "删除生成的文件")]
		public bool Build { get; set; }

		[Option('d', "dep", HelpText = "删除依赖文件")]
		public bool Dependencies { get; set; }
	}

	internal static class Program
	{
		private static readonly IWorkspace[] globals =
		{
			new CommonWorkspace(true),
			new CustomWorkspace(),
		};

		private static readonly IWorkspaceFactory[] factories =
		{
			new JetBrainsIDE(),
			new NodeJSWorkspaceFactory(),
			new VisualStudioIDE(),
		};

		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<MountOptions, InspectOptions, CleanOptions>(args)
				.WithParsed<MountOptions>(MountVFS)
				.WithParsed<CleanOptions>(Clean)
				.WithParsed<InspectOptions>(Inspect);
		}

		private static void Inspect(InspectOptions options)
		{
			Inspect(@"D:\Coding");
			Inspect(@"D:\Project");
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
			PrintGroup(groups[FileType.Build], ConsoleColor.Red, "Generated files:");
		}

		private static void Clean(CleanOptions options)
		{
			Clean(@"D:\Coding", options);
			Clean(@"D:\Project", options);
		}

		private static void Clean(string root, CleanOptions options)
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
					when options.Dependencies:
						Delete(file);
						countDeps++;
						break;
					case FileType.Build
					when options.Build:
						Delete(file);
						countBuild++;
						break;
				}
			}
			Console.WriteLine($"清理完毕，删除了{countBuild}个生成的文件/目录，和{countDeps}个依赖文件/目录");
		}

		private static void MountVFS(MountOptions options)
		{
			var map = new Dictionary<string, RootFileClassifier>
			{
				["Coding"] = new RootFileClassifier(@"D:\Coding", globals, factories),
				["Project"] = new RootFileClassifier(@"D:\Project", globals, factories),
			};

			var fs = new UnsafeCodingFS(options.Type, map);
			var wrapper = new StaticFSWrapper(fs);
			//var wrapper = new StaticFSWrapper(new AbstractFileSystem(new FileSystem()));
#if DEBUG
			wrapper.Mount("x:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
#else
			wrapper.Mount("x:\\", DokanOptions.OptimizeSingleNameSearch, new NullLogger());
#endif
		}
	}
}
