using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BenchmarkDotNet.Attributes;
using CodingFS.Workspaces;

namespace CodingFS.Benchmark;

/// <summary>
/// |         Method |       Mean |   Error |  StdDev |     Gen0 |    Gen1 | Allocated |
/// |--------------- |-----------:|--------:|--------:|---------:|--------:|----------:|
/// |   UseXmlReader |   506.3 us | 1.25 us | 1.17 us |  42.9688 |  4.8828 | 353.39 KB |
/// | UseXmlDocument | 1,004.7 us | 4.38 us | 3.89 us | 103.5156 | 68.3594 |  855.9 KB |
/// | UseXmlReaderSM |   503.0 us | 1.30 us | 1.15 us |  42.9688 |  4.8828 | 353.48 KB |
/// </summary>
[MemoryDiagnoser]
public class IdeaXMLPerf
{
	const string ideaRoot = "Resources";
	const string workspaceXml = $"{ideaRoot}/.idea/workspace.xml";

	readonly IDEAWorkspace ws = new(ideaRoot, null!);

	IEnumerable<string> OldImpl()
	{
		var document = new XmlDocument();
		document.Load(workspaceXml);

		var tsIgnores = document.SelectNodes(
			"//option[@name='exactExcludedFiles']/list//option");

		return tsIgnores.Cast<XmlNode>()
			.Select(node => node.Attributes["value"]!.Value)
			.Select(ws.ToRelative);
	}

	IEnumerable<string> NewImpl()
	{
		using var reader = XmlReader.Create(workspaceXml, IDEAWorkspace.xmlSettings);

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
				var value = reader.GetAttribute("value");
				yield return ws.ToRelative(value);
			}
			yield break;
		}
	}

	[GlobalSetup]
	public void CheckEquality()
	{
		var a = ws.ResolveWorkspace().ToArray();
		var b = NewImpl().ToArray();
		var c = OldImpl().ToArray();

		if (!a.SequenceEqual(b) || !a.SequenceEqual(c))
		{
			throw new Exception("Results not equal");
		}
	}

	[Benchmark]
	public string UseXmlReader()
	{
		return NewImpl().Last();
	}

	[Benchmark]
	public string UseXmlDocument()
	{
		return OldImpl().Last();
	}

	[Benchmark]
	public string UseXmlReaderSM()
	{
		return ws.ResolveWorkspace().Last();
	}
}
