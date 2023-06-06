using System.Collections.Generic;
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
	public void LoadWorkspace()
	{
		var dict = new Dictionary<string, RecognizeType>();
		var instance = new IDEAWorkspace(null!, "Resources", dict);
		instance.LoadWorkspace();

		Assert.Equal(87, dict.Count);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\share.js.map"]);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\proxy.spec.d.ts"]);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\verify.spec.d.ts"]);
	}

	[Fact]
	public void LoadModules()
	{
		var dict = new Dictionary<string, RecognizeType>();
		var instance = new IDEAWorkspace(null!, "Resources", dict);
		instance.LoadModules();

		Assert.Single(dict, KeyValuePair.Create("target", RecognizeType.Ignored));
	}
}
