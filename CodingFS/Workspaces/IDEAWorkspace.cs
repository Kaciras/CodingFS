using System;
using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces;

public class IDEAWorkspace : Workspace
{
	public WorkspaceKind Kind => WorkspaceKind.IDE;

	readonly CharsDictionary<RecognizeType> dict;
	readonly JetBrainsDetector detector;
	readonly string root;

	public IDEAWorkspace(JetBrainsDetector detector, string root) 
		: this(new(), root, detector) 
	{
		LoadWorkspace();
		LoadModules();
		ResolveExternalBuildSystem();
	}

	/// <summary>
	/// This constructor is only used for test/benchmark.
	/// </summary>
	internal IDEAWorkspace(
		CharsDictionary<RecognizeType> dict,
		string root, 
		JetBrainsDetector detector)
	{
		this.dict = dict;
		this.root = root;
		this.detector = detector;
	}

	public RecognizeType Recognize(string path)
	{
		var relative = new PathSpliter(path, root).Right;
		var span = relative.Span;

		if (span.SequenceEqual(".idea"))
		{
			return RecognizeType.Dependency;
		}
		if (span.EndsWith(".iml"))
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
		using var reader = XmlReaderEx.ForFile(xmlFile);

		reader.GoToAttribute("name", "exactExcludedFiles");
		reader.GoToElement("list");

		var depth = reader.Depth + 1;
		while (reader.NextInLayer(depth))
		{
			var value = reader.GetAttribute("value")!;
			dict[ToRelative(value)] = RecognizeType.Ignored;
		}
	}

	/// <summary>
	/// There may be a modules.xml file in the .idea directory records the location of the IML files.
	/// </summary>
	internal void LoadModules()
	{
		var xmlFile = Path.Join(root, ".idea/modules.xml");
		using var matcher = XmlReaderEx.ForFile(xmlFile);

		while (matcher.GoToElement("module"))
		{
			var imlFile = matcher.GetAttribute("filepath");
			imlFile = Path.Join(root, imlFile![14..]);

			var parent = Path.GetDirectoryName(imlFile.AsSpan());
			if (parent.SequenceEqual(".idea") || imlFile.Contains('/'))
			{
				ParseModuleManager(imlFile, default);
			}
			else
			{
				ParseModuleManager(imlFile, parent);
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
	/// Read excluded files from *.iml file.
	/// </summary>
	void ParseModuleManager(string imlFile, ReadOnlySpan<char> module)
	{
		using var matcher = XmlReaderEx.ForFile(imlFile);
		while (matcher.GoToElement("excludeFolder"))
		{
			var folder = matcher.GetAttribute("url")!;
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
		// Directory.GetRelativePath 对于非子路径不报错，而是原样返回
		return Path.GetRelativePath(root, value);
	}
}
