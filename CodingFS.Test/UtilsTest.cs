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

	[Fact]
	public void NormalizeSepUnsafe()
	{
		var path = @"C:\windows/a\bar/c";
		Utils.NormalizeSepUnsafe(path);

		if (OperatingSystem.IsWindows())
		{
			Assert.Equal(@"C:\windows\a\bar\c", path);
		}
		else
		{
			Assert.Equal("C:/windows/a/bar/c", path);
		}
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
