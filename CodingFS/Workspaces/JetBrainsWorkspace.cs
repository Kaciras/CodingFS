using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MoreLinq.Extensions;

namespace CodingFS.Workspaces
{
	public class JetBrainsIDE : IWorkspaceFactory
	{
		public IWorkspace? Match(string path)
		{
			if (Directory.Exists(Path.Combine(path, ".idea")))
			{
				return new JetBrainsWorkspace(path);
			}
			return null;
		}
	}

	internal class JetBrainsWorkspace : IWorkspace
	{
		private readonly string root;
		private readonly PathDict ignored;

		public JetBrainsWorkspace(string root)
		{
			this.root = root;
			ignored = new PathDict(root);
		}

		internal async Task Initialize()
		{
			var webpack = ResolveWebpackOutputPath();
			var modules = ResolveModules();
			var ebs = ResolveExternalBuildSystem();
			var workspace = ResolveWorkspace();

			// 注意PathDict不是线程安全的
			await Task.WhenAll(modules, ebs, webpack, workspace);

			if (webpack.Result != null)
			{
				ignored.AddIgnore(webpack.Result);
			}
			modules.Result.ForEach(ignored.AddIgnore);
			ebs.Result.ForEach(ignored.AddIgnore);
			workspace.Result.ForEach(ignored.AddIgnore);
		}

		public RecognizeType Recognize(string path)
		{
			var relative = Path.GetRelativePath(root, path);

			if (relative == ".idea")
			{
				return RecognizeType.Dependency;
			}
			if (Path.GetExtension(relative) == ".iml")
			{
				return RecognizeType.Dependency;
			}
			return ignored.Recognize(relative);
		}

		/// <summary>
		/// JB的项目在文件夹下的.idea目录里存储配置，其中的 workspace.xml
		/// 文件保存了与工作区域相关的信息，包括排除的文件等。
		/// 
		/// 这个方法从workspace.xml里读取被排除的文件列表。
		/// </summary>
		/// <returns>被排除的文件</returns>
		private async Task<IEnumerable<string>> ResolveWorkspace()
		{
			var xmlFile = Path.Join(root, ".idea/workspace.xml");
			var doc = new XmlDocument();
			doc.LoadXml(await File.ReadAllTextAsync(xmlFile));

			var tsIgnores = doc.SelectNodes(
				"//component[@name='TypeScriptGeneratedFilesManager']" +
				"/option[@name='exactExcludedFiles']/list//option");

			return tsIgnores.Cast<XmlNode>()
				.Select(node => ToRelative(node.Attributes["value"].Value))
				.SkipWhile(Path.IsPathRooted)
				.SkipWhile(path => path.StartsWith("..") || path.StartsWith("node_modules"));
		}

		/// <summary>
		/// 在.idea目录下可能存在一个modules.xml文件，里面记录了IML文件的位置。
		/// </summary>
		private async Task<IEnumerable<string>> ResolveModules()
		{
			var xmlFile = Path.Join(root, ".idea/modules.xml");
			if (!File.Exists(xmlFile))
			{
				return Enumerable.Empty<string>();
			}

			var doc = new XmlDocument();
			doc.Load(await File.ReadAllTextAsync(xmlFile));

			var modules = doc.SelectNodes("//component[@name='ProjectModuleManager']/modules//module");
			var flatten = Enumerable.Empty<string>();

			for (int i = 0; i < modules.Count; i++)
			{
				var imlFile = modules[i].Attributes["filepath"].Value[14..];
				imlFile = Path.Join(root, imlFile);

				var parent = Path.GetDirectoryName(imlFile);
				if (parent == ".idea" || imlFile.Contains('/'))
				{
					flatten = flatten.Concat(await ParseModuleManager(imlFile, null));
				}
				else
				{
					flatten = flatten.Concat(await ParseModuleManager(imlFile, parent));
				}
			}

			return flatten;
		}

		/// <summary>
		/// 在IDEA用户配置目录的 system/external_build_system/modules 下还有iml文件。
		/// </summary>
		private async Task<IEnumerable<string>> ResolveExternalBuildSystem()
		{
			var configPath = GetUserConfigStore();
			if (configPath == null)
			{
				return Enumerable.Empty<string>();
			}

			// 计算项目在 external_build_system 里对应的文件夹，计算方法见：
			// https://github.com/JetBrains/intellij-community/blob/734efbef5b75dfda517731ca39fb404404fbe182/platform/platform-api/src/com/intellij/openapi/project/ProjectUtil.kt#L146

			var cache = JavaStringHashcode(root).ToString("x2");
			cache = Path.GetFileName(root) + "." + cache;
			configPath = Path.Join(configPath, "system/external_build_system", cache, "modules");

			if (!Directory.Exists(configPath))
			{
				return Enumerable.Empty<string>();
			}

			var flatten = Enumerable.Empty<string>();

			foreach (var file in Directory.EnumerateFiles(configPath))
			{
				string? moduleDirectory = null;

				var stem = Path.GetFileNameWithoutExtension(file);
				if (Path.GetFileName(root) != stem)
				{
					moduleDirectory = stem;
				}
				flatten = flatten.Concat(await ParseModuleManager(file, moduleDirectory));
			}

			return flatten;
		}

		private string? GetUserConfigStore()
		{
			var IDEA_DIR_RE = new Regex(@"\.IntelliJIdea([0-9.]+)");
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			// 查找最新版的配置目录，JB的产品在更新了次版本号之后会创建一个新的配置文件夹
			string? configPath = null;
			var version = string.Empty;

			foreach (var name in Directory.EnumerateDirectories(home))
			{
				var match = IDEA_DIR_RE.Match(name);
				if (match.Success)
				{
					// 假定版本号只有1位数才能直接比较
					var nv = match.Groups[1].Value;
					if (nv.CompareTo(version) > 0)
					{
						version = nv;
						configPath = name;
					}
				}
			}

			return configPath;
		}

		/// <summary>
		/// 从模块配置文件（.iml）里读取被忽略的文件列表。
		/// </summary>
		/// <param name="imlFile"></param>
		/// <param name="module"></param>
		private async Task<IEnumerable<string>> ParseModuleManager(string imlFile, string? module)
		{
			if (!File.Exists(imlFile))
			{
				return Enumerable.Empty<string>();
			}
			var doc = new XmlDocument();
			doc.Load(await File.ReadAllTextAsync(imlFile));
			return GetExcludesFromIml(doc, module);
		}

		private IEnumerable<string> GetExcludesFromIml(XmlDocument doc, string? module)
		{
			var nodes = doc.SelectNodes("//component[@name='NewModuleRootManager']/content//excludeFolder");
			for (int i = 0; i < nodes.Count; i++)
			{
				var folder = nodes[i].Attributes["url"].Value;

				if (!folder.StartsWith("file://$MODULE_DIR$/"))
				{
					throw new Exception("断言失败");
				}
				folder = folder.Substring(20);

				if (module == null)
				{
					yield return folder;
				}
				yield return Path.Join(module, folder);
			}
		}

		// TODO: 异步等待进程
		private async Task<string?> ResolveWebpackOutputPath()
		{
			var xmlFile = Path.Join(root, ".idea/misc.xml");
			if (!File.Exists(xmlFile))
			{
				return null;
			}

			var doc = new XmlDocument();
			doc.Load(await File.ReadAllTextAsync(xmlFile));

			var option = doc.SelectNodes("//component[@name='WebPackConfiguration']/option[@name='path']");
			if (option.Count == 0)
			{
				return null;
			}
			var config = option.Item(0).Attributes["value"].Value;
			config = config.Replace("$PROJECT_DIR$", root);

			// webpack.config.js 里可能动态计算输出目录，故必须运行此文件。
			var startInfo = new ProcessStartInfo
			{
				FileName = "node",
				WorkingDirectory = root,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			startInfo.ArgumentList.Add(Path.Join(Environment.CurrentDirectory, "Scripts/webpack-output.js"));
			startInfo.ArgumentList.Add(config);

			var process = Process.Start(startInfo);
			var taskSource = new TaskCompletionSource<string>();

			process.Exited += (_, __) =>
			{
				if (process.ExitCode == 0)
				{
					taskSource.SetResult(process.StandardOutput.ReadToEnd());
				}
				else
				{
					var message = process.StandardError.ReadToEnd();
					taskSource.SetException(new Exception(message));
				}
			};

			return await taskSource.Task;
		}

		private string ToRelative(string value)
		{
			value = value.Replace("$PROJECT_DIR$", root);

			// 绝对路径也有可能是项目下的文件
			// Path.GetRelativePath 对于非子路径不报错，而是原样返回
			return Path.GetRelativePath(root, value);
		}

		internal static int JavaStringHashcode(string str)
		{
			return str.Aggregate(0, (h, c) => 31 * h + c);
		}
	}
}
