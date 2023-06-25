using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodingFS.Test;

public class UtilsTest
{
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
