using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace CodingFS.Filter
{
	public class JetBrainsIDE : ClassifierFactory
	{
		public Classifier? Match(string path)
		{
			if (Directory.Exists(Path.Combine(path, ".idea")))
			{
				return new JetBrainsClassifier(path);
			}
			return null;
		}
	}

	internal class JetBrainsClassifier : Classifier
	{
		readonly string root;

		readonly RecognizedFileMap ignored;

		public JetBrainsClassifier(string root)
		{
			this.root = root;
			ignored = new RecognizedFileMap(root);
			ResloveWorkspace();
			ResloveModules();
			ResloveExternalBuildSystem();
		}

		private void ResloveWorkspace()
		{
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, ".idea/workspace.xml"));

			foreach (var item in ParseWorkspace(root, doc))
			{
				ignored.Add(item, RecognizeType.Ignored);
			}
		}

		/// <summary>
		/// JB的项目在文件夹下的.idea目录里存储配置，其中的 workspace.xml
		/// 文件保存了与工作区域相关的信息，包括排除的文件等。
		/// 
		/// 这个方法从workspace.xml里读取被排除的文件列表。
		/// </summary>
		/// <param name="projectRoot">项目文件夹路径</param>
		/// <param name="doc">解析后的workspace.xml</param>
		/// <returns>被排除的文件</returns>
		internal static IEnumerable<string> ParseWorkspace(string projectRoot, XmlDocument doc)
		{
			var tsIgnores = doc.SelectNodes(
				"//component[@name='TypeScriptGeneratedFilesManager']" +
				"/option[@name='exactExcludedFiles']/list//option");

			// 这个XML解析库竟然还是非泛型的
			for (int i = 0; i < tsIgnores.Count; i++)
			{
				var value = tsIgnores[i].Attributes["value"].Value;

				if (value.StartsWith("$PROJECT_DIR$/"))
				{
					value = value[14..];
				}

				// 绝对路径也有可能是项目下的文件
				if (Path.IsPathRooted(value))
				{
					var rooted = value;
					value = Path.GetRelativePath(projectRoot, value);

					// Path.GetRelativePath 对于非子路径不报错，而是原样返回
					if (rooted == value) continue;
				}

				// 项目之外的不关心，node_modules已经由Node识别器处理了
				if (value.StartsWith("..") ||
					value.StartsWith("node_modules"))
				{
					continue;
				}

				yield return value;
			}
		}

		/// <summary>
		/// 在.idea目录下可能存在一个modules.xml文件，里面记录了IML文件的位置。
		/// </summary>
		private void ResloveModules()
		{
			var xmlFile = Path.Join(root, ".idea/modules.xml");

			if (!File.Exists(xmlFile))
			{
				return;
			}
			var doc = new XmlDocument();
			doc.Load(xmlFile);

			var modules = doc.SelectNodes("//component[@name='ProjectModuleManager']/modules//module");
			for (int i = 0; i < modules.Count; i++)
			{
				var imlFile = modules[i].Attributes["filepath"].Value[14..];
				var parent = Path.GetDirectoryName(imlFile);

				if (parent == ".idea" || imlFile.Contains('/'))
				{
					ParseModuleManager(imlFile, null);
				}
				else
				{
					ParseModuleManager(imlFile, parent);
				}
			}
		}

		/// <summary>
		/// 在IDEA用户配置目录的 system/external_build_system/modules 下还有iml文件。
		/// 
		/// TODO：2019.2 开始找不到这个文件夹了
		/// </summary>
		private void ResloveExternalBuildSystem()
		{
			var IDEA_DIR_RE = new Regex(@"\.IntelliJIdea([0-9.]+)");
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			// 查找最新版的配置目录，JB的产品在更新了次版本号之后会创建一个新的配置文件夹
			string? configPath = null;
			var version = string.Empty;

			foreach (var name in Directory.EnumerateDirectories(home))
			{
				var match = IDEA_DIR_RE.Match(name);
				if (match != null)
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

			if (configPath == null)
			{
				return;
			}

			// 计算项目在 external_build_system 里对应的文件夹，计算方法见：
			// https://github.com/JetBrains/intellij-community/blob/734efbef5b75dfda517731ca39fb404404fbe182/platform/platform-api/src/com/intellij/openapi/project/ProjectUtil.kt#L146

			var cache = JavaStringHashcode(root).ToString();
			cache = Path.GetFileName(root) + "." + cache;
			configPath = Path.Join(configPath, "system/external_build_system", cache, "modules");

			if (!Directory.Exists(configPath))
			{
				return;
			}
			foreach (var file in Directory.EnumerateFiles(configPath))
			{
				string? moduleDirectory = null;

				var stem = Path.GetFileNameWithoutExtension(file);
				if (Path.GetFileName(root) != stem)
				{
					moduleDirectory = stem;
				}
				ParseModuleManager(file, moduleDirectory);
			}
		}

		/// <summary>
		/// 从模块配置文件（.iml）里读取被忽略的文件列表。
		/// </summary>
		/// <param name="iml"></param>
		/// <param name="module"></param>
		private void ParseModuleManager(string iml, string? module)
		{
			iml = Path.Join(root, iml);

			if (!File.Exists(iml))
			{
				return;
			}

			var doc = new XmlDocument();
			doc.Load(iml);

			var excludes = doc.SelectNodes("//component[@name='NewModuleRootManager']/content//excludeFolder");
			for (int i = 0; i < excludes.Count; i++)
			{
				var dir = excludes[i].Attributes["url"].Value;

				if (!dir.StartsWith("file://$MODULE_DIR$/"))
				{
					throw new Exception("断言失败");
				}
				dir = dir.Substring(20);
				if (module != null)
				{
					dir = Path.Join(module, dir);
				}
				ignored.Add(dir, RecognizeType.Ignored);
			}
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

		internal static int JavaStringHashcode(string str)
		{
			return str.ToCharArray().Aggregate(0, (h, c) => 31 * h + c);
		}
	}
}
