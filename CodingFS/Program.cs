using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

	}

	internal static class Program
	{
		private static readonly IWorkspaceFactory[] factories =
		{
			new JetBrainsIDE(),
			new NodeJSWorkspaceFactory(),
			new VisualStudioIDE(),
		};

		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<MountOptions, InspectOptions>(args)
				.WithParsed<MountOptions>(MountVFS)
				.WithParsed<InspectOptions>(Inspect);
		}

		private static void Inspect(InspectOptions options)
		{
			//var dir = @"D:\Coding\Python\OpsTool";

			//var matches = factories
			//		.Select(f => f.Match(dir))
			//		.Where(x => x != null)!
			//		.ToList<Classifier>();

			//var ins = new ProjectInspector(dir, matches);
			//ins.PrintFiles();
			Inspect(@"D:\Coding");
			Inspect(@"D:\Project");
		}

		private static void Inspect(string root)
		{
			var dirs = Directory.EnumerateDirectories(root);
			foreach (var dir in dirs)
			{
				var matches = factories
					.Select(f => f.Match(dir))
					.Where(x => x != null)!
					.ToList<IWorkspace>();

				if (matches.Count == 0)
				{
					Inspect(dir);
				}
				else
				{
					Console.WriteLine($"项目{Path.GetFileName(dir)}:");
					matches.Add(new CommonWorkspace());
					var ins = new ProjectInspector(dir, matches);
					ins.PrintFiles();
					Console.WriteLine();
				}
			}
		}

		private static void MountVFS(MountOptions options)
		{
			var fs = new UnsafeCodingFS(options.Type, @"D:\Coding", @"D:\Project");
			var wrapper = new StaticFSWrapper(fs);

#if !DEBUG
			fs.Mount("x:\\", DokanOptions.OptimizeSingleNameSearch, new NullLogger());
#else
			fs.Mount("x:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
#endif
		}
	}
}
