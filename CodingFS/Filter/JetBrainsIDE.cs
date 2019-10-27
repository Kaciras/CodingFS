using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using System.Xml;

namespace CodingFS.Filter
{
	public class JetBrainsIDE : ClassifierFactory
	{
		public Classifier? TryMatch(string path)
		{
			if (Directory.Exists(Path.Combine(path, ".idea")))
			{
				return new JetBrainsClassifier(path);
			}
			return null;
		}
	}

	class JetBrainsClassifier : Classifier
	{
		readonly string root;

		readonly PathTrie<RecognizeType> ignored = new PathTrie<RecognizeType>(RecognizeType.Uncertain);

		public JetBrainsClassifier(string root)
		{
			this.root = root;
			ResloveWorkspace();
			ResloveModules();
		}

		void ResloveWorkspace()
		{
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, ".idea/workspace.xml"));

			var tsIgnores = doc.SelectNodes(
				"component[@name='TypeScriptGeneratedFilesManager']" +
				"/option[@name='exactExcludedFiles']/list//option");

			// 这个XML解析库竟然还是非泛型的
			for (int i = 0; i < tsIgnores.Count; i++)
			{
				var value = tsIgnores[i].Attributes["value"].Value;

				if (value.StartsWith("$PROJECT_DIR$/", StringComparison.Ordinal))
				{
					value = value[14..];
				}
				if (Path.IsPathRooted(value))
				{
					value = Path.GetRelativePath(root, value);
				}

				// Path.GetRelativePath 对于非子路径不报错，反而原样返回，这是什么脑残设计？
				if (Path.IsPathRooted(value))
				{
					continue;
				}
				if (!value.StartsWith("..", StringComparison.Ordinal))
				{
					ignored.Add(value, RecognizeType.Ignored);
				}
			}
		}

		void ResloveModules()
		{
			var xmlFile = Path.Join(root, ".idea/modules.xml");

			if(!File.Exists(xmlFile))
			{
				return;
			}
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, xmlFile));

			var modules = doc.SelectNodes("component[@name='ProjectModuleManager']/modules//module");
			for (int i = 0; i < modules.Count; i++)
			{
				var imlFile = modules[i].Attributes["filepath"].Value[14..];

			}
		}


		public RecognizeType RecognizeDirectory(string path)
		{
			return RecognizeType.Dependency;
		}

		internal static int JavaStringHashcode(string str)
		{
			return str.ToCharArray().Aggregate(0, (hash, ch) => 31 * hash + ch);
		}
	}
}
