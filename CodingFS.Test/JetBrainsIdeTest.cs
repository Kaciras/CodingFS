using System.Linq;
using CodingFS.Workspaces;
using Xunit;

namespace CodingFS.Test;

public class JetBrainsIdeTest
{
	[Theory]
	[InlineData("😗🍏🍎🚗", -734144743)]
	[InlineData("the quick brown fox jumps over a lazy dog", 2013091971)]
	[InlineData("", 0)]
	public void JavaStringHashcode(string value, int hash)
	{
		var actual = JetBrainsDetector.JavaStringHashcode(value);
		Assert.Equal(hash, actual);
	}

	[Fact]
	public void ResolveWorkspace()
	{
		var instance = new IDEAWorkspace("Resources", null);
		var values = instance.ResolveWorkspace().ToArray();

		Assert.Equal(87, values.Length);
		Assert.Equal(@"__tests__\share.js.map", values[0]);
		Assert.Equal(@"__tests__\proxy.spec.d.ts", values[10]);
		Assert.Equal(@"__tests__\verify.spec.d.ts", values[^1]);
	}
}
