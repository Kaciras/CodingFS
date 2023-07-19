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

	public WorkspaceKind Kind { get; }

	readonly string name;

	public CustomWorkspace() : this("Custom", WorkspaceKind.Other) {}

	public CustomWorkspace(string name, WorkspaceKind kind) : this(name, kind, new()) {}

	public CustomWorkspace(
		string name,
		WorkspaceKind kind, 
		Dictionary<string, RecognizeType> dict)
	{
		this.name = name;
		Kind = kind;
		Dict = dict;
	}

	public RecognizeType Recognize(string path) => Dict.GetValueOrDefault(path);
}
