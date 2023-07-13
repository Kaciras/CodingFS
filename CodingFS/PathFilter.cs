using System.Collections.Generic;
using System.IO;
using DokanNet;

namespace CodingFS;

public interface PathFilter
{
	void HandleChange(string file);

	string MapPath(string path);

	IEnumerable<FileInformation> ListFiles(string dir);
}
