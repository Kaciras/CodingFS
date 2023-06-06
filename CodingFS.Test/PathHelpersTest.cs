using System.Collections.Generic;
using Xunit;

namespace CodingFS.Test;

public class PathHelpersTest
{
	[InlineData("😗🍏🍎🚗", new string[] { "😗🍏🍎🚗" })]
	[InlineData("a/bar/c", new string[] { "a", "bar", "c" })]
	[Theory]
	public void Split(string path, string[] components)
	{
		var s = new PathComponentSpliter(path);

		var list = new List<string>();
		while (s.HasNext)
		{
			list.Add(new string(s.SplitNext().Span));
		}

		Assert.Equal(components, list.ToArray());
	}

	[InlineData("/a/bar/c", "/a", "/a/bar")]
	[InlineData("a/bar/c", "a", "a/bar")]
	[Theory]
	public void Relative(string path, string root, string next)
	{
		var s = new PathComponentSpliter(path);

		s.Relative(root);
		Assert.Equal(root, new string(s.Left.Span));

		s.SplitNext();
		Assert.Equal(next, new string(s.Left.Span));
	}

	[Fact]
	public void NormalizeSepUnsafe()
	{
		var s = new PathComponentSpliter(@"/a\bar\c");
		s.NormalizeSepUnsafe();
		Assert.Equal("/a/bar/c", new string(s.Right.Span));
	}
}
