using System;
using System.IO;
using System.Linq;
using Moq;
using Xunit;

namespace CodingFS.Test;

public sealed class MappedPathFilterTest
{
	static readonly string SEP = Path.DirectorySeparatorChar.ToString();

	[Fact]
	public void NoSubDirectorySupport()
	{
		var inner = new Mock<PathFilter>().Object;
		var filter = new MappedPathFilter();

		Assert.Throws<ArgumentException>(() => filter.Set("foo/bar", inner));
	}

	[Fact]
	public void ListSubFiltersEmpty()
	{
		var filter = new MappedPathFilter();
		Assert.Empty(filter.ListFiles(SEP));
	}

	[Fact]
	public void ListSubFilters()
	{
		var filter = new MappedPathFilter();
		var inner = new Mock<PathFilter>().Object;
		filter.Set("Foo", inner);
		filter.Set("Bar", inner);

		var items = filter.ListFiles(SEP).ToArray();
		Assert.Equal(2, items.Length);
	}

	[Fact]
	public void NotInTheMap()
	{
		var filter = new MappedPathFilter();
		Assert.Throws<FileNotFoundException>(() => filter.MapPath("/foo/bar"));
	}

	[Fact]
	public void DispatchToInner()
	{
		var filter = new MappedPathFilter();
		var inner = new Mock<PathFilter>();
		filter.Set("Foo", inner.Object);

		inner.Setup(i => i.MapPath("Bar")).Returns("Baz");
		Assert.Equal("Baz", filter.MapPath("/Foo/Bar"));
	}

	[Fact]
	public void ListFiles()
	{
		var filter = new MappedPathFilter();
		var inner = new Mock<PathFilter>();
		filter.Set("Foo", inner.Object);

		filter.ListFiles("/Foo/Bar");

		inner.Verify(i => i.ListFiles("Bar"));
	}
}
