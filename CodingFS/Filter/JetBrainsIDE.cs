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

		readonly PathTrie<RecognizeType> ignored = new PathTrie<RecognizeType>(RecognizeType.Uncertain);

		public JetBrainsClassifier(string root)
		{
			this.root = root;
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
				if (Path.IsPathRooted(value))
				{
					value = Path.GetRelativePath(projectRoot, value);
				}

				// Path.GetRelativePath 对于非子路径不报错，反而原样返回，这是什么脑残设计？
				if (Path.IsPathRooted(value))
				{
					continue;
				}
				if (value.StartsWith(".."))
				{
					continue;
				}
				if (!value.StartsWith("node_modules"))
				{
					yield return value;
				}
			}
		}

		private void ResloveModules()
		{
			var xmlFile = Path.Join(root, ".idea/modules.xml");

			if (!File.Exists(xmlFile))
			{
				return;
			}
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, xmlFile));

			var modules = doc.SelectNodes("component[@name='ProjectModuleManager']/modules//module");
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

		private void ResloveExternalBuildSystem()
		{
			var IDEA_DIR_RE = new Regex(@"\.IntelliJIdea([0-9.]+)");
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			string? configPath = null;
			var version = string.Empty;

			foreach (var name in Directory.EnumerateDirectories(home))
			{
				var match = IDEA_DIR_RE.Match(name);
				if (match != null)
				{
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
		/// 从模块的配置文件里读取排除的目录，配置文件可以是以 .iml 为扩展名或在IDEA配置目录的
		/// system/external_build_system/modules 下。
		/// </summary>
		/// <param name="iml"></param>
		/// <param name="module"></param>
		private void ParseModuleManager(string iml, string? module)
		{
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, iml));

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
			return ignored.Get(relative, RecognizeType.NotCare);
		}

		internal static int JavaStringHashcode(string str)
		{
			return str.ToCharArray().Aggregate(0, (h, c) => 31 * h + c);
		}
	}
}
