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
	/// Temp file, Logs, build output, cache (little improve performance)...
	/// </summary>
	Ignored = 1,

	/// <summary>
	/// 3rd party file, cache (significantly improve performance)...
	/// </summary>
	Dependency = 2,
}
