using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace CodingFS.Workspaces;

public class IDEAWorkspace : Workspace
{
	readonly JetBrainsDetector detector;
	readonly string root;
	readonly PathDict ignored;

	internal IDEAWorkspace(JetBrainsDetector detector, string root)
	{
		this.detector = detector;
		this.root = root;
		ignored = new PathDict(root);

		foreach (var item in ResolveModules())
		{
			ignored.AddIgnore(item);
		}
		foreach (var item in ResolveWorkspace())
		{
			ignored.AddIgnore(item);
		}
		foreach (var item in ResolveExternalBuildSystem())
		{
			ignored.AddIgnore(item);
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

	/// <summary>
	/// Find excluded file patterns from ".idea/workspace.xml".
	/// </summary>
	private IEnumerable<string> ResolveWorkspace()
	{
		var xmlFile = Path.Join(root, ".idea/workspace.xml");
		var doc = new XmlDocument();
		doc.Load(xmlFile);

		var tsIgnores = doc.SelectNodes(
			"//option[@name='exactExcludedFiles']/list//option");

		return tsIgnores.Cast<XmlNode>()
			.Select(node => ToRelative(node.Attributes["value"]!.Value));
	}

	/// <summary>
	/// 在.idea目录下可能存在一个modules.xml文件，里面记录了IML文件的位置。
	/// </summary>
	private IEnumerable<string> ResolveModules()
	{
		var xmlFile = Path.Join(root, ".idea/modules.xml");
		if (!File.Exists(xmlFile))
		{
			var imlFile = Path.Join(root, Path.GetFileName(root) + ".iml");
			return ParseModuleManager(imlFile, null);
		}

		var doc = new XmlDocument();
		doc.Load(xmlFile);

		var modules = doc.SelectNodes("//module")!;
		var flatten = Enumerable.Empty<string>();

		foreach (XmlNode module in modules)
		{
			var imlFile = module.Attributes["filepath"].Value[14..];
			imlFile = Path.Join(root, imlFile);

			var parent = Path.GetDirectoryName(imlFile);
			if (parent == ".idea" || imlFile.Contains('/'))
			{
				flatten = flatten.Concat(ParseModuleManager(imlFile, null));
			}
			else
			{
				flatten = flatten.Concat(ParseModuleManager(imlFile, parent));
			}
		}

		return flatten;
	}

	/// <summary>
	/// 在IDEA用户配置目录的 system/external_build_system/modules 下还有iml文件。
	/// </summary>
	private IEnumerable<string> ResolveExternalBuildSystem()
	{
		// 计算项目在 external_build_system 里对应的文件夹，计算方法见：
		// https://github.com/JetBrains/intellij-community/blob/734efbef5b75dfda517731ca39fb404404fbe182/platform/platform-api/src/com/intellij/openapi/project/ProjectUtil.kt#L146
		var extModules = detector.EBSModuleFiles(root);
		if (extModules == null)
		{
			return Enumerable.Empty<string>();
		}

		var flatten = Enumerable.Empty<string>();

		foreach (var file in Directory.EnumerateFiles(extModules))
		{
			string? moduleDirectory = null;
			var stem = Path.GetFileNameWithoutExtension(file);
			if (Path.GetFileName(root) != stem)
			{
				moduleDirectory = stem;
			}
			flatten = flatten.Concat(ParseModuleManager(file, moduleDirectory));
		}

		return flatten;
	}

	/// <summary>
	/// 从模块配置文件（.iml）里读取被忽略的文件列表。
	/// </summary>
	/// <param name="imlFile"></param>
	/// <param name="module"></param>
	private IEnumerable<string> ParseModuleManager(string imlFile, string? module)
	{
		if (!File.Exists(imlFile))
		{
			yield break;
		}
		var doc = new XmlDocument();
		doc.Load(imlFile);

		foreach (XmlNode node in doc.SelectNodes("//excludeFolder")!)
		{
			var folder = node.Attributes["url"].Value;
			if (!folder.StartsWith("file://$MODULE_DIR$/"))
			{
				throw new Exception("断言失败");
			}
			folder = folder[20..];
			yield return module == null ? folder : Path.Join(module, folder);
		}
	}

	private string ToRelative(string value)
	{
		value = value.Replace("$PROJECT_DIR$", root);

		// 绝对路径也有可能是项目下的文件
		// Path.GetRelativePath 对于非子路径不报错，而是原样返回
		return Path.GetRelativePath(root, value);
	}
}
