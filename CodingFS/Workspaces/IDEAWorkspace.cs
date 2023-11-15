using System;
using System.Collections.Generic;
using System.IO;

namespace CodingFS.Workspaces;

public sealed class IDEAWorkspace : Workspace
{
	readonly CharsDictionary<RecognizeType> dict;
	readonly JetBrainsDetector detector;
	readonly string root;

	public IDEAWorkspace(JetBrainsDetector detector, string root)
		: this([], root, detector)
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
		try
		{
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
		catch (FileNotFoundException)
		{
			// Only if the user delete the file, ignored for robust.
		}
	}

	/// <summary>
	/// There may be a modules.xml file in the .idea directory records the location of the IML files.
	/// </summary>
	internal void LoadModules()
	{
		try
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
		catch (FileNotFoundException)
		{
			var name = Path.GetFileName(root);
			ParseModuleManager(Path.Join(root, name + ".iml"), default);
		}
	}

	internal void ResolveExternalBuildSystem()
	{
		var ext = detector.ExternalBuildSystem(root);
		if (ext == null)
		{
			return;
		}
		try
		{

			foreach (var file in Directory.EnumerateFiles(ext))
			{
				ReadOnlySpan<char> module = default;
				var stem = Path.GetFileNameWithoutExtension(file);
				if (Path.GetFileName(root) != stem)
				{
					module = stem;
				}
				ParseModuleManager(file, module);
			}
		}
		catch (DirectoryNotFoundException)
		{
			// Project may not have EBS, ignored for robust.
		}
	}

	/// <summary>
	/// Read excluded files from *.iml file.
	/// </summary>
	void ParseModuleManager(string imlFile, ReadOnlySpan<char> module)
	{
		try
		{
			using var matcher = XmlReaderEx.ForFile(imlFile);
			while (matcher.GoToElement("excludeFolder"))
			{
				var url = matcher.GetAttribute("url")!;
				dict[ToRelative(url, module)] = RecognizeType.Ignored;
			}
		}
		catch (FileNotFoundException)
		{
			// Only if the user delete the file, ignored for robust.
		}
	}

	static ReadOnlyMemory<char> ToRelative(string value)
	{
		if (value.StartsWith("$PROJECT_DIR$/"))
		{
			var relative = value.AsMemory()[14..];
			PathSpliter.NormalizeSepUnsafe(relative);
			return relative;
		}
		else
		{
			throw new Exception("Expect relative path only.");
		}
	}

	static ReadOnlyMemory<char> ToRelative(string value, ReadOnlySpan<char> prefix)
	{
		if (!value.StartsWith("file://$MODULE_DIR$/"))
		{
			throw new Exception("Expect relative path only.");
		}
		var relative = value.AsMemory()[20..];
		PathSpliter.NormalizeSepUnsafe(relative);

		return prefix.IsEmpty
			? relative
			: Path.Join(prefix, relative.Span).AsMemory();
	}
}
