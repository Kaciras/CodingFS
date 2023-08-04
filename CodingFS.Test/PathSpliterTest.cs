using System;
using System.Collections.Generic;
using Xunit;

namespace CodingFS.Test;

public sealed class PathSpliterTest
{
	[InlineData("A:/foo/bar", "A:/foo/", "bar")]
	[InlineData("A:/foo/bar", "A:/foo", "bar")]
	[InlineData("A:/foo/bar", "", "A:/foo/bar")]
	[InlineData("A:/foo/bar", "A:/foo/bar", ".")]
	[Theory]
	public void GetRelative(string path, string relativeTo, string expected)
	{
		var relative = PathSpliter.GetRelative(relativeTo, path);
		Assert.Equal(expected, relative.ToString());
	}

	[InlineData("foo/bar", "", false)]
	[InlineData("A:/foo/bar", "", true)]
	[InlineData("/foo/bar", "", true)]
	[InlineData("/", "", true)]
	[InlineData("/foo/bar", "/foo", true)]
	[Theory]
	public void IsRooted(string path, string relativeTo, bool expected)
	{
		Assert.Equal(expected, new PathSpliter(path, relativeTo).IsRooted);
	}

	[InlineData("😗🍏🍎🚗", new string[] { "😗🍏🍎🚗" })]
	[InlineData("", new string[] { "" })]
	[InlineData("/a", new string[] { "/", "a" })]
	[InlineData("a/bar/c", new string[] { "a", "bar", "c" })]
	[InlineData("A:/b/c", new string[] { "A:/", "b", "c" })]
	[Theory]
	public void SplitNext(string path, string[] components)
	{
		var s = new PathSpliter(path);

		var list = new List<string>();
		while (s.HasNext)
		{
			list.Add(new string(s.SplitNext().Span));
		}

		Assert.Equal(components, list.ToArray());
	}

	[InlineData("/a/bar/c", "/", "/a")]
	[InlineData("/a/bar/c", "/a", "/a/bar")]
	[InlineData("a/bar/c", "a", "a/bar")]
	[InlineData("A:/bar/c", "A:/", "A:/bar")]
	[InlineData("D:/Coding", "D:/Coding", "D:/Coding")]
	[Theory]
	public void Relative(string path, string root, string prev)
	{
		var s = new PathSpliter(path, root);
		Assert.Equal(root.AsMemory(), s.Left, Utils.memComparator);

		if (s.HasNext)
		{
			s.SplitNext();
		}

		Assert.Equal(prev.AsMemory(), s.Left, Utils.memComparator);
	}

	[Fact]
	public void RightWhenDrained() => Assert.Throws<ArgumentOutOfRangeException>(() =>
	{
		var s = new PathSpliter("foo");
		s.SplitNext();
		return s.Right;
	});

	[InlineData("a/bar/c", "", "a/bar/c")]
	[InlineData("a/bar/c", "a/bar", "c")]
	[InlineData("", "", "")]
	[Theory]
	public void Right(string path, string root, string expected)
	{
		var s = new PathSpliter(path, root);
		Assert.Equal(expected.AsMemory(), s.Right, Utils.memComparator);
	}
}
