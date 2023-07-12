using System.Xml;

namespace CodingFS;

static class XmlReaderEx
{
	internal static readonly XmlReaderSettings settings = new()
	{
		IgnoreComments = true,
		IgnoreWhitespace = true,
	};

	public static XmlReader ForFile(string file)
	{
		return XmlReader.Create(file, settings);
	}

	public static bool NextInLayer(this XmlReader reader,int depth)
	{
		while (reader.Read())
		{
			if (reader.Depth < depth)
			{
				return false;
			}
			if (reader.Depth == depth)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GoToElement(this XmlReader reader)
	{
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GoToElement(this XmlReader reader,string tagName)
	{
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element && reader.Name == tagName)
			{
				return true;
			}
		}
		return false;
	}

	public static void GoToAttribute(this XmlReader reader, string name, string value)
	{
		while (reader.Read() && reader.GetAttribute(name) != value) ;
	}
}
