using System.Collections.Generic;
using System.IO;
using System.Linq;
using DokanNet;

namespace CodingFS.FUSE;

sealed class FilteredDokan : UnsafeRedirectDokan
{
	readonly string name;
	readonly PathFilter filter;

	/// <summary>
	/// Create a new CodingFS with name, the name will displayed as volume label.
	/// </summary>
	/// <param name="name"></param>
	public FilteredDokan(string name, PathFilter filter)
	{
		this.name = name;
		this.filter = filter;
	}

	protected override string GetPath(string value)
	{
		return filter.MapPath(value);
	}

	public override NtStatus GetVolumeInformation(
		out string volumeLabel,
		out FileSystemFeatures features,
		out string fileSystemName,
		out uint maximumComponentLength,
		IDokanFileInfo info)
	{
		volumeLabel = name;
		features = FileSystemFeatures.UnicodeOnDisk
			| FileSystemFeatures.CaseSensitiveSearch
			| FileSystemFeatures.PersistentAcls
			| FileSystemFeatures.SupportsRemoteStorage
			| FileSystemFeatures.CasePreservedNames;
		fileSystemName = "FilteredFS";
		maximumComponentLength = 256;
		return DokanResult.Success;
	}
	public override NtStatus FindFiles(
			string fileName,
			out IList<FileInformation> files,
			IDokanFileInfo info)
	{
		files = filter.ListFiles(fileName).ToArray();
		return DokanResult.Success;
	}

	public override NtStatus CreateFile(string fileName, DokanNet.FileAccess access,
		FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
	{
		if (fileName == @"\")
		{
			return DokanResult.Success;
		}
		return base.CreateFile(fileName, access, share, mode, options, attributes, info);
	}
}
