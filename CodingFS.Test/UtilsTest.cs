using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingFS.Workspaces;
using Xunit;

namespace CodingFS.Test;

public class UtilsTest
{
	[Theory]
	[InlineData("😗🍏🍎🚗", -734144743)]
	[InlineData("the quick brown fox jumps over a lazy dog", 2013091971)]
	[InlineData("", 0)]
	public void JavaStringHashcode(string value, int hash)
	{
		Assert.Equal(hash, Utils.JavaStringHashcode(value));
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
}
