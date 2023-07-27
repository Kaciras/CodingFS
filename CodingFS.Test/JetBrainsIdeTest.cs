using CodingFS.Workspaces;
using Xunit;

namespace CodingFS.Test;

public sealed class JetBrainsIDETest
{
	[Fact]
	public void LoadWorkspace()
	{
		var dict = new CharsDictionary<RecognizeType>();
		var instance = new IDEAWorkspace(dict, "Resources", null!);
		instance.LoadWorkspace();

		Assert.Equal(87, dict.Count);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\share.js.map"]);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\proxy.spec.d.ts"]);
		Assert.Equal(RecognizeType.Ignored, dict[@"__tests__\verify.spec.d.ts"]);
	}

	[Fact]
	public void LoadModules()
	{
		var dict = new CharsDictionary<RecognizeType>();
		var instance = new IDEAWorkspace(dict, "Resources", null!);
		instance.LoadModules();

		Assert.Single(dict);
		Assert.Equal(RecognizeType.Ignored, dict["target"]);
	}
}
