using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace CodingFS.Workspaces;

using TypeDict = IReadOnlyDictionary<string, RecognizeType>;

/// <summary>
/// A simple workspace that you can manually associate type to some files.
/// </summary>
public sealed class CustomWorkspace(string name, TypeDict dict) : Workspace
{
	public ReadOnlySpan<char> Name => name;

	readonly TypeDict dict = dict.ToFrozenDictionary();
	readonly string name = name;

	public RecognizeType Recognize(string path) => dict.GetValueOrDefault(path);
}
