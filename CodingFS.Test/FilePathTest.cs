using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CodingFS.Test
{
	public class FilePathTest
	{
		[Theory]
		[InlineData("/foo/bar/中文", "/foo/bar/中文", true)]	// 相同的值
		[InlineData("/foo/bar", @"\foo/bar\", true)]			// 两种分隔符
		[InlineData("/foo/bar", "/foo/bar/", true)]				// 尾部分隔符
		[InlineData("/foo/bar", "foo/bar", false)]				// 首部分隔符
		[InlineData("/foo/bar", "/AnotherPath", false)]			// 不同的值
		[InlineData("/foo/bar", "/fOO/Bar", false)]				// 大小写敏感
		public void EqualsAndHashCode(string a, string b, bool isEqual)
		{
			FilePath path = new FilePath(a), other = new FilePath(b);

			Assert.Equal(isEqual, path.Equals(other));
			Assert.Equal(isEqual, other.Equals(path));

			Assert.Equal(isEqual, path.GetHashCode() == other.GetHashCode());
		}
	}
}
