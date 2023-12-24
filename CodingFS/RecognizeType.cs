using System;

namespace CodingFS;

[Flags]
public enum RecognizeType
{
	/// <summary>
	/// Source file, or the file is not managed by the workspace.
	/// </summary>
	NotCare = 0,

	/// <summary>
	/// 3rd party file, cache (significantly improve performance)...
	/// All files in a folder of this type are also of this type.
	/// </summary>
	Dependency = 2,

	/// <summary>
	/// Temp file, Logs, build output, cache (little improve performance)...
	/// All files in a folder of this type are also of this type.
	/// </summary>
	Ignored = 4,
}
