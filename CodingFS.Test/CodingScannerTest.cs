using System;
using System.Collections.Generic;
using Xunit;

namespace CodingFS.Test;

public sealed class CodingScannerTest
{
	readonly List<string> checkedPaths = new();

	void RecordPath(DetectContxt ctx)
	{
		checkedPaths.Add(ctx.Path);
	}

	[Fact]
	public void DetectWorkspaces()
	{
		var i = new CodingScanner("/foo", Array.Empty<Workspace>(), new Detector[] { RecordPath });
		i.GetWorkspaces("/foo/CSharp/CodingFS/CodingFS/bin/Debug/net7.0");

		Assert.Equal(7, checkedPaths.Count);
		Assert.Equal("/foo", checkedPaths[0]);
		Assert.Equal("/foo/CSharp", checkedPaths[1]);
		Assert.Equal("/foo/CSharp/CodingFS", checkedPaths[2]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS", checkedPaths[3]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin", checkedPaths[4]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin/Debug", checkedPaths[5]);
		Assert.Equal("/foo/CSharp/CodingFS/CodingFS/bin/Debug/net7.0", checkedPaths[6]);
	}

	[Fact]
	public void Cache()
	{
		var i = new CodingScanner("/foo", Array.Empty<Workspace>(), new Detector[] { RecordPath });
		i.GetWorkspaces("/foo/bar");

		checkedPaths.Clear();
		i.GetWorkspaces("/foo/bar/baz");

		Assert.Single(checkedPaths);
		Assert.Equal("/foo/bar/baz", checkedPaths[0]);
	}

	[Fact]
	public void Invalid()
	{
		var i = new CodingScanner("/foo", Array.Empty<Workspace>(), new Detector[] { RecordPath });
		i.GetWorkspaces("/foo/bar/baz");

	}

	[Fact]
	public void MaxDepth()
	{
		var i = new CodingScanner("/foo", Array.Empty<Workspace>(), new Detector[] { RecordPath });
		i.MaxDepth = 3;
		i.GetWorkspaces("/foo/CSharp/CodingFS/CodingFS/bin/Debug/net7.0");

		Assert.Equal(3, checkedPaths.Count);
		Assert.Equal("/foo", checkedPaths[0]);
		Assert.Equal("/foo/CSharp", checkedPaths[1]);
		Assert.Equal("/foo/CSharp/CodingFS", checkedPaths[2]);
	}
}
