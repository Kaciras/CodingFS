using System;
using System.IO;
using System.Linq;

namespace CodingFS.Workspaces;

public sealed class MSBuildProject : Workspace
{
	public static readonly string[] SDK_CSHARP = { "obj" , "bin" };
	public static readonly string[] SDK_CPP = { "Debug", "Release", "x64", "win32" };

	public WorkspaceKind Kind => WorkspaceKind.IDE;

	public VisualStudioWorkspace Solution { get; }

	public string Folder { get; }

	public string[] SDK { get; }

	public MSBuildProject(VisualStudioWorkspace solution, string folder, string file)
	{
		Solution = solution;
		Folder = folder;
		SDK = Path.GetExtension(file.AsSpan()) switch
		{
			".csproj" => SDK_CSHARP,
			".vcxproj" => SDK_CPP,
			_ => throw new NotImplementedException(),
		};
	}

	public RecognizeType Recognize(string path)
	{
		return SDK.Contains(path) 
			? RecognizeType.Ignored : RecognizeType.NotCare;
	}
}
