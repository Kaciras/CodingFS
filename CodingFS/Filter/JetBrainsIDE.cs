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
			if (Directory.Exists(Path.Combine(path, ".git")))
			{
				return new JetBrainsClassifier(path);
			}
			return null;
		}
	}

	class JetBrainsClassifier : Classifier
	{
		readonly string root;
		readonly PathTrie ignored = new PathTrie(RecognizeType.Ignored);

		public JetBrainsClassifier(string root)
		{
			this.root = root;
		}

		void ResloveWorkspace()
		{
			var doc = new XmlDocument();
			doc.Load(Path.Join(root, ".idea/workspace.xml"));
			var tsIgores = doc.SelectNodes("component[@name='TypeScriptGeneratedFilesManager']/option[@name='exactExcludedFiles']/list//option");

			foreach (XmlNode item in tsIgores)
			{
				var value = item.Attributes["value"].Value;
				if (value.StartsWith("$PROJECT_DIR$/", StringComparison.Ordinal))
				{
					value = value[14..];
				}
				if(Path.IsPathRooted(value))
				{
					value = Path.GetRelativePath(root, value);
				}

				// Path.GetRelativePath 对于非子路径不报错，反而原样返回，这是什么脑残设计？
				if (Path.IsPathRooted(value))
				{
					continue;
				}
				if(!value.StartsWith("..", StringComparison.Ordinal))
				{
					ignored.Add(value);
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
			foreach (XmlNode item in modules)
			{
				var imlFile = item.Attributes["filepath"].Value[14..];

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
