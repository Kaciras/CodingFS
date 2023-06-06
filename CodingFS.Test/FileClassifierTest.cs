using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodingFS.Test;

public class FileClassifierTest
{
	[Fact]
	public void Test()
	{
		var paths = new List<string>();
		var i = new FileClassifier("/foo", new WorkspaceFactory[] { ctx => paths.Add(ctx.Path) }, Array.Empty<Workspace>());
		i.GetWorkspaces("/foo/CSharp/CodingFS/CodingFS/bin/Debug/net7.0");

		Assert.Equal(6, paths.Count);
		Assert.Equal("/foo/CSharp", paths[0]);
		Assert.Equal("/foo/CSharp/CodingFS", paths[1]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS", paths[2]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin", paths[3]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin/Debug", paths[4]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin/Debug/net7.0", paths[5]);
	}
}
