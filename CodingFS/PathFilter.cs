using System.Collections.Generic;
using DokanNet;

namespace CodingFS;

/// <summary>
/// View of the local directories, used to create vitrual file system.
/// </summary>
public interface PathFilter
{
	/// <summary>
	/// Get the real path of the path in the vitrual drive.
	/// </summary>
	string MapPath(string path);

	/// <summary>
	/// Read and filter the contents of the directory.
	/// </summary>
	IEnumerable<FileInformation> ListFiles(string directory);
}
