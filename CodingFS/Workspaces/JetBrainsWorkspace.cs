using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using MoreLinq.Extensions;

namespace CodingFS.Workspaces;

internal class JetBrainsWorkspace : Workspace
{
	public static Workspace? Match(string path)
	{
		return Directory.Exists(Path.Combine(path, ".idea")) ? new JetBrainsWorkspace(path) : null;
	}

	private readonly string root;
	private readonly PathDict ignored;

	public JetBrainsWorkspace(string root)
	{
		this.root = root;
		ignored = new PathDict(root);

		var modules = ResolveModules();
		//var ebs = ResolveExternalBuildSystem();
		var workspace = ResolveWorkspace();

		modules.ForEach(ignored.AddIgnore);
		//ebs.ForEach(ignored.AddIgnore);
		workspace.ForEach(ignored.AddIgnore);
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
	private IEnumerable<string> ResolveWorkspace()
	{
		var xmlFile = Path.Join(root, ".idea/workspace.xml");
		var doc = new XmlDocument();
		doc.Load(xmlFile);

		var tsIgnores = doc.SelectNodes(
			"//component[@name='TypeScriptGeneratedFilesManager']" +
			"/option[@name='exactExcludedFiles']/list//option");

		return tsIgnores.Cast<XmlNode>()
			.Select(node => ToRelative(node.Attributes["value"]!.Value))
			.SkipWhile(Path.IsPathRooted)
			.SkipWhile(path => path.StartsWith("..") || path.StartsWith("node_modules"));
	}

	/// <summary>
	/// 在.idea目录下可能存在一个modules.xml文件，里面记录了IML文件的位置。
	/// </summary>
	private IEnumerable<string> ResolveModules()
	{
		var xmlFile = Path.Join(root, ".idea/modules.xml");
		if (!File.Exists(xmlFile))
		{
			return Enumerable.Empty<string>();
		}

		var doc = new XmlDocument();
		doc.Load(xmlFile);

		var modules = doc.SelectNodes("//component[@name='ProjectModuleManager']/modules//module")!;
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

		var nodes = doc.SelectNodes("//component[@name='NewModuleRootManager']/content//excludeFolder");
		for (int i = 0; i < nodes!.Count; i++)
		{
			var folder = nodes[i].Attributes["url"].Value;

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

	internal static int JavaStringHashcode(string str)
	{
		return str.Aggregate(0, (h, c) => 31 * h + c);
	}
}
