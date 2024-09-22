using System;

namespace CodingFS;

// Although a file can only be of one type, make this enum flag-able is useful for filtering.
[Flags]
public enum FileType
{
	// Meanless, but enum should have a default value.
	None = 0,

	Source = 1,
	Dependency = 2,
	Generated = 4,
}
