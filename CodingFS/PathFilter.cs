using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingFS;

public interface PathFilter
{
	IEnumerable<string> ListFiles(string dir, FileType type);

	void HandleChange(string file);

	PathFilterContext Prepare();
}

public interface PathFilterContext
{
	FileType GetFileType(string file);
}
