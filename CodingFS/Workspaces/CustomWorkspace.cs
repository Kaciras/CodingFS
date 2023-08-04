using System;
using System.Collections.Generic;

namespace CodingFS.Workspaces;

/// <summary>
/// A simple workspace that you can manually associate type to some files.
/// </summary>
public sealed class CustomWorkspace : Workspace
{
	public Dictionary<string, RecognizeType> Dict { get; }

	public ReadOnlySpan<char> Name => name;

	readonly string name;

	public CustomWorkspace() : this("Custom") {}

	public CustomWorkspace(string name) : this(name, new()) {}

	public CustomWorkspace(
		string name,
		Dictionary<string, RecognizeType> dict)
	{
		this.name = name;
		Dict = dict;
	}

	public RecognizeType Recognize(string path) => Dict.GetValueOrDefault(path);
}
