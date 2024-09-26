using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CodingFS.FUSE;
using Xunit;

namespace CodingFS.Test.FUSE;

file sealed class DokanImpl : RedirectDokan
{
	const string PREFIX = "CodingFS-Test-";

	public readonly DirectoryInfo directory;

	public DokanImpl()
	{
		directory = Directory.CreateTempSubdirectory(PREFIX);
	}

	protected override string GetPath(string fileName)
	{
		return Path.Join(directory.FullName, fileName);
	}
}

public sealed class RedirectVolume : DokanMounter
{
	public readonly DirectoryInfo directory;

	public RedirectVolume() : base("v:", new DokanExceptionWrapper(new DokanImpl()), true)
	{
		WaitForReady();

		var wrapper = Unsafe.As<DokanExceptionWrapper>(VFS);
		directory = Unsafe.As<DokanImpl>(wrapper.Native).directory;
	}

	public override void Dispose()
	{
		base.Dispose();
		directory.Delete(true);
	}

	public string MapPath(string fileName)
	{
		return Path.Join(directory.FullName, fileName);
	}

	public DirectoryInfo CreateDirectory(string fileName)
	{
		return directory.CreateSubdirectory(fileName);
	}

	public void CreateFile(string fileName, string text)
	{
		File.WriteAllText(MapPath(fileName), text);
	}
}

/// <summary>
/// RedirectDokan drived from Dokan.Net, we only test methods that modified by us.
/// </summary>
public sealed class RedirectDokanTest : IClassFixture<RedirectVolume>
{
	readonly RedirectVolume mock;

	public RedirectDokanTest(RedirectVolume mock)
	{
		this.mock = mock;
		mock.directory.Delete(true);
		mock.directory.Create();
		mock.CreateDirectory("foo");
	}

	[Fact]
	public void ReadDirectoryAsFile()
	{
		Assert.Throws<UnauthorizedAccessException>(() => File.ReadAllBytes(@"v:\foo"));
	}

	[Fact]
	public void WriteDirectoryAsFile()
	{
		Assert.Throws<UnauthorizedAccessException>(() => File.WriteAllBytes(@"v:\foo", []));
	}

	[Fact]
	public void DeleteDirectoryAsFile()
	{
		Assert.Throws<UnauthorizedAccessException>(() => File.Delete(@"v:\foo"));
	}

	[Fact]
	public void MoveDirectoryAsFile()
	{
		Assert.Throws<FileNotFoundException>(() => File.Move(@"v:\foo", @"v:\bar"));
	}

	[Fact]
	public void DeleteDirectory()
	{
		Directory.Delete(@"v:\foo");
		Assert.False(Directory.Exists(mock.MapPath("foo")));
	}

	[Fact]
	public void ListDirectory()
	{
		var list = Directory.EnumerateFileSystemEntries("v:").ToList();
		Assert.Equal(@"v:\foo", Assert.Single(list));
	}

	[Fact]
	public void WriteFile()
	{
		File.WriteAllText(@"v:\wf.txt", "123");
		Assert.Equal("123", File.ReadAllText(mock.MapPath("wf.txt")));
	}
}
