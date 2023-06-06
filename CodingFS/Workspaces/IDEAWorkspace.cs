using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CodingFS.Workspaces;

public class IDEAWorkspace : Workspace
{
	internal static readonly XmlReaderSettings xmlSettings = new()
	{
		IgnoreComments = true,
		IgnoreWhitespace = true,
	};
	

	readonly JetBrainsDetector detector;
	readonly string root;
	readonly Dictionary<string, RecognizeType> dict;

	internal IDEAWorkspace(JetBrainsDetector detector, string root) 
		: this(detector, root, new()) 
	{
		LoadWorkspace();
		LoadModules();
		ResolveExternalBuildSystem();
	}

	/// <summary>
	/// This constructor is only used for test/benchmark.
	/// </summary>
	internal IDEAWorkspace(
		JetBrainsDetector detector, 
		string root, 
		Dictionary<string, RecognizeType> dict)
	{
		this.root = root;
		this.dict = dict;
		this.detector = detector;
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
		return dict.GetValueOrDefault(relative);
	}

	/// <summary>
	/// Find excluded file patterns from ".idea/workspace.xml".
	/// </summary>
	internal void LoadWorkspace()
	{
		var xmlFile = Path.Join(root, ".idea/workspace.xml");
		using var matcher = new FastXmlMatcher(xmlFile);

		matcher.MoveToAttribute("name", "exactExcludedFiles");
		matcher.MoveToElement("list");

		var depth = matcher.Reader.Depth + 1;
		while (matcher.NextInLayer(depth))
		{
			var value = matcher.Reader.GetAttribute("value")!;
			dict[ToRelative(value)] = RecognizeType.Ignored;
		}
	}

	/// <summary>
	/// 在.idea目录下可能存在一个modules.xml文件，里面记录了IML文件的位置。
	/// </summary>
	internal void LoadModules()
	{
		var xmlFile = Path.Join(root, ".idea/modules.xml");
		if (!File.Exists(xmlFile))
		{
			var imlFile = Path.Join(root, Path.GetFileName(root) + ".iml");
			ParseModuleManager(imlFile, null);
		}
		else
		{
			using var matcher = new FastXmlMatcher(xmlFile);
			while (matcher.MoveToElement("module"))
			{
				var imlFile = matcher.Reader.GetAttribute("filepath");
				imlFile = Path.Join(root, imlFile![14..]);

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
	}

	internal void ResolveExternalBuildSystem()
	{
		var ext = detector.EBSModuleFiles(root);
		if (ext != null)
		{
			foreach (var file in Directory.EnumerateFiles(ext))
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
	}

	/// <summary>
	/// 从模块配置文件（.iml）里读取被忽略的文件列表。
	/// </summary>
	/// <param name="imlFile"></param>
	/// <param name="module"></param>
	void ParseModuleManager(string imlFile, string? module)
	{
		if (!File.Exists(imlFile))
		{
			return;
		}
		using var matcher = new FastXmlMatcher(imlFile);
		while (matcher.MoveToElement("excludeFolder"))
		{
			var folder = matcher.Reader.GetAttribute("url")!;
			if (!folder.StartsWith("file://$MODULE_DIR$/"))
			{
				throw new Exception("断言失败");
			}
			folder = folder[20..];
			var path = module == null ? folder : Path.Join(module, folder);
			dict[path] = RecognizeType.Ignored;
		}
	}

	internal string ToRelative(string value)
	{
		value = value.Replace("$PROJECT_DIR$", root);

		// 绝对路径也有可能是项目下的文件
		// Path.GetRelativePath 对于非子路径不报错，而是原样返回
		return Path.GetRelativePath(root, value);
	}
}
