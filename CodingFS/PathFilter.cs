using System.Collections.Generic;
using DokanNet;

namespace CodingFS;

/// <summary>
/// View of the local directories, used to create vitrual file system.
/// </summary>
public interface PathFilter
{
	/// <summary>
	/// Called when a file/directory changed. You can invalid cache here.
	/// </summary>
	void HandleChange(string path);

	/// <summary>
	/// Get the real path of the vitrual path.
	/// </summary>
	string MapPath(string path);

	/// <summary>
	/// Read and filter the contents of the directory.
	/// </summary>
	IEnumerable<FileInformation> ListFiles(string directory);
}
