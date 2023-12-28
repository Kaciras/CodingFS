using System;
using CodingFS.Helper;
using Xunit;

namespace CodingFS.Test;

public sealed class UtilsTest
{
	[InlineData("the quick brown fox jumps over a lazy dog", 2013091971)]
	[InlineData("", 0)]
	[InlineData("😗🍏🍎🚗", -734144743)]
	[Theory]
	public void JavaStringHashCode(string value, int hash)
	{
		Assert.Equal(hash, Utils.JavaStringHashCode(value));
	}

	[InlineData(RecognizeType.NotCare, FileType.Source)]
	[InlineData(RecognizeType.Dependency, FileType.Dependency)]
	[InlineData(RecognizeType.Ignored, FileType.Generated)]
	[InlineData(RecognizeType.Ignored | RecognizeType.Dependency, FileType.Dependency)]
	[Theory]
	public void ToFileType(RecognizeType input, FileType expected)
	{
		Assert.Equal(expected, input.ToFileType());
	}

	[InlineData(".A.A.A", 2, 3)]
	[InlineData("", 2, -1)]
	[Theory]
	public void IndexOfNth(string text, int n, int expected)
	{
		Assert.Equal(expected, text.IndexOfNth('A', n));
	}
}
