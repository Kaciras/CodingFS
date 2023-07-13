using System.Collections.Generic;
using System.IO;

namespace CodingFS;

public interface PathFilter
{
	void HandleChange(string file);

	string MapPath(string path);

	IEnumerable<FileSystemInfo> ListFiles(string dir);
}
