using System;
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
}
