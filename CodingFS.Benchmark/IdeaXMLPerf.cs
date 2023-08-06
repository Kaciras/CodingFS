using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using BenchmarkDotNet.Attributes;
using CodingFS.Workspaces;

namespace CodingFS.Benchmark;

/// <summary>
/// |         Method |     Mean |   Error |  StdDev |     Gen0 |    Gen1 | Allocated |
/// |--------------- |---------:|--------:|--------:|---------:|--------:|----------:|
/// |        Current | 502.6 us | 0.90 us | 0.70 us |  42.9688 |  6.8359 | 353.48 KB |
/// |   UseXmlReader | 518.2 us | 1.58 us | 1.48 us |  42.9688 |  6.8359 | 353.41 KB |
/// | UseXmlDocument | 990.3 us | 6.62 us | 6.19 us | 103.5156 | 68.3594 | 855.79 KB |
/// </summary>
[MemoryDiagnoser]
public class IdeaXMLPerf
{
	const string ideaRoot = "Resources";
	const string workspaceXml = $"{ideaRoot}/.idea/workspace.xml";

	readonly Dictionary<string, RecognizeType> dict = new();
	readonly CharsDictionary<RecognizeType> memDict = new();

	readonly IDEAWorkspace ws;

	public IdeaXMLPerf()
	{
		ws = new(memDict, ideaRoot, null!);
	}

	void OldXmlDocumentImpl()
	{
		var document = new XmlDocument();
		document.Load(workspaceXml);

		var tsIgnores = document.SelectNodes(
			"//option[@name='exactExcludedFiles']/list//option");

		foreach (XmlNode node in tsIgnores!)
		{
			var path = node.Attributes!["value"]!.Value;
			dict[ToRelative(path)] = RecognizeType.Ignored;
		}
	}

	void XmlReaderWithoutEx()
	{
		using var reader = XmlReaderEx.ForFile(workspaceXml);
		while (reader.Read())
		{
			if (!reader.IsStartElement())
			{
				continue;
			}
			if (reader.GetAttribute("name") != "exactExcludedFiles")
			{
				continue;
			}
			if (!reader.Read() || reader.Name != "list")
			{
				throw new Exception();
			}

			while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
			{
				var path = reader.GetAttribute("value")!;
				dict[ToRelative(path)] = RecognizeType.Ignored;
			}
			return;
		}
	}

	static string ToRelative(string value)
	{
		value = value.Replace("$PROJECT_DIR$", ideaRoot);
		return Path.GetRelativePath(ideaRoot, value);
	}

	Dictionary<string, RecognizeType> Run(Action action)
	{
		dict.Clear();
		action();
		return dict;
	}

	[Benchmark]
	public Dictionary<string, RecognizeType> Current() => Run(ws.LoadWorkspace);

	[Benchmark]
	public Dictionary<string, RecognizeType> UseXmlReader() => Run(XmlReaderWithoutEx);

	[Benchmark]
	public Dictionary<string, RecognizeType> UseXmlDocument() => Run(OldXmlDocumentImpl);
}
