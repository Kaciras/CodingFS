using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CodingFS.Workspaces;

file struct VSCodeSettings
{
	[JsonPropertyName("files.exclude")]
	public IDictionary<string, bool> Exclude { get; set; }

	public VSCodeSettings()
	{
		Exclude = ImmutableDictionary<string, bool>.Empty;
	}

	public static VSCodeSettings Parse(string file)
	{
		try
		{
			using var stream = File.OpenRead(file);
			return JsonSerializer.Deserialize<VSCodeSettings>(stream)!;
		}
		catch (FileNotFoundException)
		{
			return new VSCodeSettings();
		}
	}
}

internal class VSCodeWorkspace : Workspace
{
	public static void Match(DetectContxt ctx)
	{
		if (Utils.IsDir(ctx.Path, ".vscode"))
		{
			ctx.AddWorkspace(new VSCodeWorkspace(ctx.Path));
		}
	}

	public WorkspaceKind Kind => WorkspaceKind.IDE;

	public string Folder { get; }

	readonly Matcher excludes = new();

	// https://code.visualstudio.com/docs/editor/glob-patterns#_glob-pattern-syntax
	// https://github.com/microsoft/vscode/blob/main/src/vs/base/common/glob.ts

	public VSCodeWorkspace(string folder)
	{
		Folder = folder;
		
		var file = Path.Join(folder, ".vscode/settings.json");
		var setting = VSCodeSettings.Parse(file);
		foreach (var (k, v) in setting.Exclude)
		{
			var path = k.TrimStart('/');
			if (v)
			{
				excludes.AddInclude(path);
			}
			else
			{
				excludes.AddExclude(path);
			}
		}
	}

	public RecognizeType Recognize(string path)
	{
		if (path == ".vscode")
		{
			return RecognizeType.Dependency;
		}

		var relative = Path.GetRelativePath(Folder, path);
		if (excludes.Match(relative).HasMatches)
		{
			return RecognizeType.Ignored;
		}
		else
		{
			return RecognizeType.NotCare;
		}
	}
}
