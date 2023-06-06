using System.Xml;

namespace CodingFS;

internal readonly ref struct FastXmlMatcher
{
	internal static readonly XmlReaderSettings xmlSettings = new()
	{
		IgnoreComments = true,
		IgnoreWhitespace = true,
	};

	public readonly XmlReader Reader;

	public FastXmlMatcher(string file)
	{
		Reader = XmlReader.Create(file, xmlSettings);
	}

	public readonly void Dispose() => Reader.Dispose();

	public readonly bool NextInLayer(int depth)
	{
		while (Reader.Read())
		{
			if (Reader.Depth < depth)
			{
				return false;
			}
			if (Reader.Depth == depth)
			{
				return true;
			}
		}
		return false;
	}

	public readonly bool MoveToElement()
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				return true;
			}
		}
		return false;
	}

	public readonly bool MoveToElement(string tagName)
	{
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == tagName)
			{
				return true;
			}
		}
		return false;
	}

	public readonly void MoveToAttribute(string name, string value)
	{
		while (Reader.Read() && Reader.GetAttribute(name) != value) ;
	}
}
