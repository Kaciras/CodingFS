using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CodingFS.Filter;
using CodingFS.Test.Properties;
using Xunit;

namespace CodingFS.Test
{
	public class JetBrainsIdeTest
	{
		[Theory]
		[InlineData("😗🍏🍎🚗", -734144743)]
		[InlineData("the quick brown fox jumps over a lazy dog", 2013091971)]
		[InlineData("", 0)]
		public void JavaStringHashcode(string value, int hash)
		{
			var actual = JetBrainsClassifier.JavaStringHashcode(value);
			Assert.Equal(hash, actual);
		}

		[Fact]
		public void ParseWorkspace()
		{
			var doc = new XmlDocument();
			doc.LoadXml(Resources.workspace);

			var ignores = JetBrainsClassifier.ParseWorkspace(@"D:\Project\Blog\WebServer", doc).ToList();

			Assert.Equal(4, ignores.Count);
			Assert.Equal("packages/devtool/lib/webpack/HooksInspectPlugin.js", ignores[0]);
			Assert.Equal("packages/devtool/lib/webpack/HooksInspectPlugin.js.map", ignores[1]);
			Assert.Equal("packages/image/__tests__/coding-filter-tests.js.map", ignores[2]);
			Assert.Equal("packages/kxc-server/index.js.map", ignores[3]);
		}

		[Fact]
		public void ParseModules()
		{

		}
	}
}
