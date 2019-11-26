using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CodingFS.Test
{
	public class FilePathTest
	{
		[Fact]
		public void HashCode()
		{
			var a = "/foo/bar/中文";
			var b = "/foo/bar/中文/";
			var c = @"\foo/bar\中文\";

			var d = "foo/bar/中文";
			var e = "another/path";

			Assert.Equal(new FilePath(a).GetHashCode(), new FilePath(b).GetHashCode());
			Assert.Equal(new FilePath(a).GetHashCode(), new FilePath(c).GetHashCode());
			Assert.Equal(new FilePath(b).GetHashCode(), new FilePath(c).GetHashCode());

			Assert.NotEqual(new FilePath(a).GetHashCode(), new FilePath(d).GetHashCode());
			Assert.NotEqual(new FilePath(a).GetHashCode(), new FilePath(e).GetHashCode());
			Assert.NotEqual(new FilePath(d).GetHashCode(), new FilePath(e).GetHashCode());
		}
	}
}
