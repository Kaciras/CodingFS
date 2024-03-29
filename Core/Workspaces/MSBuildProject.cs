﻿using System;
using System.IO;
using CodingFS.Helper;

namespace CodingFS.Workspaces;

public sealed class MSBuildProject : Workspace
{
	public static readonly string[] SDK_CPP = ["Debug", "Release", "x64", "win32"];
	public static readonly string[] SDK_CSHARP = ["obj", "bin"];

	public VisualStudioWorkspace Solution { get; }

	public string Folder { get; }

	public string[] SDK { get; }

	public MSBuildProject(VisualStudioWorkspace solution, string folder, string file)
	{
		Solution = solution;
		Folder = folder;
		SDK = Path.GetExtension(file.AsSpan()) switch
		{
			"" => [], // Solution Items.
			".csproj" => SDK_CSHARP,
			".vcxproj" => SDK_CPP,
			_ => throw new NotImplementedException($"Unsupported MSBuild project type: {file}"),
		};
	}

	public RecognizeType Recognize(string path)
	{
		var relative = PathSpliter.GetRelative(Folder, path);
		return Utils.IndexOfSpan(SDK, relative) != -1 ? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
