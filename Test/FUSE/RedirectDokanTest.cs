using System;
using System.IO;
using System.Runtime.CompilerServices;
using CodingFS.FUSE;
using Xunit;

namespace CodingFS.Test.FUSE;

sealed class DokanImpl : RedirectDokan
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
	readonly DirectoryInfo directory;

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

	public void CreateDirectory(string fileName)
	{
		directory.CreateSubdirectory(fileName);
	}

	public void CreateFile(string fileName, string text)
	{
		File.WriteAllText(Path.Join(directory.FullName, fileName), text);
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
	}

	[Fact]
	public void ReadDirectoryAsFile()
	{
		mock.CreateDirectory("foo");
		Assert.Throws<UnauthorizedAccessException>(() => File.ReadAllBytes(@"v:\foo"));
	}
}
