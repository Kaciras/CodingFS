using System;
using CodingFS.Filter;
using Xunit;

namespace CodingFS.Test
{
	public class UnitTest
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
	}
}
