using System;

namespace CodingFS;

// Although a file can only be of one type, make this enum flag-able is useful for filtering.
[Flags]
public enum FileType
{
	Source = 0,
	Dependency = 1,
	Generated = 2,
}
