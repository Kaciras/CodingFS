using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CodingFS.Helper;

namespace CodingFS.Workspaces;

public sealed class IDEAWorkspace : Workspace
{
	readonly CharsDictionary<RecognizeType> dict;
	readonly JetBrainsDetector detector;
	readonly string root;

	public IDEAWorkspace(JetBrainsDetector detector, string root)
		: this([], root, detector)
	{
		LoadModules();
		LoadWorkspace();
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
		try
		{
			using var reader = XmlReaderEx.ForFile(xmlFile);

			reader.GoToAttribute("name", "exactExcludedFiles");
			reader.GoToElement("list");

			var depth = reader.Depth + 1;
			while (reader.NextInLayer(depth))
			{
				var value = reader.GetAttribute("value")!;
				dict[ResolveProjectPath(value)] = RecognizeType.Ignored;
			}
		}
		catch (FileNotFoundException)
		{
			// Only if the user delete the file, ignored for robust.
		}
	}

	/// <summary>
	/// Find excluded file patterns from module descriptor files.
	/// </summary>
	internal void LoadModules()
	{
		try
		{
			// Lookup modules.xml from .idea folder, it contains IML file paths.
			var xmlFile = Path.Join(root, ".idea/modules.xml");
			foreach (var imlFile in GetIMLFiles(xmlFile))
			{
				var path = Path.Join(root, imlFile);
				ParseModuleManager(path, GetModuleDir(imlFile));
			}
		}
		catch (FileNotFoundException)
		{
			// IML file also can be in .idea folder, and MODULE_DIR is the project root.
			var name = Path.GetFileName(root);
			ParseModuleManager(Path.Join(root, name + ".iml"), default);
		}

		// Projects use external build system also have modules.xml in AppData.
		var dir = detector.ExternalBuildSystem(root);
		if (dir != null)
		{
			var xmlFile = Path.Join(dir, "project/modules.xml");
			try
			{
				foreach (var imlFile in GetIMLFiles(xmlFile))
				{
					var stem = Path.GetFileNameWithoutExtension(imlFile);
					var imlEx = Path.Join(dir, "modules", stem + ".xml");
					ParseModuleManager(imlEx, GetModuleDir(imlFile));
				}
			}
			catch (DirectoryNotFoundException)
			{
				// Project may not use EBS, or used deleted it, ignored for robust.
			}
		}
	}

	IEnumerable<string> GetIMLFiles(string modulesXml)
	{
		using var matcher = XmlReaderEx.ForFile(modulesXml);
		while (matcher.GoToElement("module"))
		{
			yield return matcher.GetAttribute("filepath")![14..];
		}
	}

	/*
	 * $MODULE_DIR$ is point to the directry of the IML file,
	 * or project root if the IML file is in .idea folder.
	 */
	ReadOnlySpan<char> GetModuleDir(string imlFile)
	{
		var parent = Path.GetDirectoryName(imlFile.AsSpan());
		return parent.SequenceEqual(".idea") ? default : parent;
	}

	/// <summary>
	/// Read excluded files from module descriptor file.
	/// </summary>
	void ParseModuleManager(string imlFile, ReadOnlySpan<char> module)
	{
		try
		{
			using var matcher = XmlReaderEx.ForFile(imlFile);
			var excludeFolders = new List<string>();

			while (matcher.Read())
			{
				if (matcher.NodeType == XmlNodeType.Element &&
					matcher.Name == "excludeFolder")
				{
					excludeFolders.Add(matcher.GetAttribute("url")!);
				}
			}
			
			foreach (var url in excludeFolders)
			{
				dict[ResolveModulePath(url, module)] = RecognizeType.Ignored;
			}
		}
		catch (FileNotFoundException)
		{
			// Only if the user delete the file, ignored for robust.
		}
	}

	static ReadOnlyMemory<char> ResolveProjectPath(string value)
	{
		if (value.StartsWith("$PROJECT_DIR$/"))
		{
			var relative = value.AsMemory(14);
			PathSpliter.NormalizeSepUnsafe(relative);
			return relative;
		}
		else
		{
			throw new Exception("Expect relative path only.");
		}
	}

	ReadOnlyMemory<char> ResolveModulePath(string value, ReadOnlySpan<char> moduleDir)
	{
		if (!value.StartsWith("file://$MODULE_DIR$/"))
		{
			throw new Exception("Path does not start with $MODULE_DIR$");
		}

		value = value["file://$MODULE_DIR$/".Length..];
		if (moduleDir.Length == 0)
		{
			PathSpliter.NormalizeSepUnsafe(value);
			return value.AsMemory();
		}

		// Join and resolve ".." in path, just a bit dirty.
		var absolute = Path.Join(moduleDir, value);
		absolute = Path.GetFullPath(absolute);
		return absolute.AsMemory(Environment.CurrentDirectory.Length + 1);
	}
}
