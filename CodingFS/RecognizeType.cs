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
	/// </summary>
	Dependency = 2,

	/// <summary>
	/// Temp file, Logs, build output, cache (little improve performance)...
	/// </summary>
	Ignored = 4,
}
