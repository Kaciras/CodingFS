using System;
using Xunit;

namespace CodingFS.Test;

public sealed class StackStringBuilderTest
{
	[InlineData(1145141919, "44417a9f")]
	[InlineData(45781033, "2ba9029")]
	[Theory]
	public void AppendFormat(int number, string expected)
	{
		var builder = new StackStringBuilder(stackalloc char[20]);
		builder.AppendFormat(number, "x2");
		Assert.Equal(expected, builder.ToString());
	}

	[Fact]
	public void AppendFormatOutOfCapacity()
	{
		Assert.Throws<IndexOutOfRangeException>(() =>
		{
			var builder = new StackStringBuilder(stackalloc char[1]);
			builder.AppendFormat(1145141919, "x2");
		});
	}
}
