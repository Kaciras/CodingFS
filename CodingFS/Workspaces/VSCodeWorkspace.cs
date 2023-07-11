using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileSystemGlobbing;

namespace CodingFS.Workspaces;

file class VSCodeSettings
{
	[JsonPropertyName("files.exclude")]
	public Dictionary<string, bool> Exclude { get; set; }

	public static VSCodeSettings Parse(string file)
	{
		return JsonSerializer.Deserialize<VSCodeSettings>(File.OpenRead(file))!;
	}
}

internal class VSCodeWorkspace : Workspace
{
	public static void Match(DetectContxt ctx)
	{
		if (Directory.Exists(Path.Join(ctx.Path, ".vscode")))
		{
			ctx.AddWorkspace(new VSCodeWorkspace(ctx.Path));
		}
	}

	public WorkspaceKind Kind => WorkspaceKind.IDE;

	public string Folder { get; }

	readonly Matcher excludes = new();

	public VSCodeWorkspace(string folder)
	{
		Folder = folder;
		try
		{
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
		catch (FileNotFoundException)
		{
			// ignore
		}
	}

	public RecognizeType Recognize(string relative)
	{
		if (relative == ".vscode")
		{
			return RecognizeType.Dependency;
		}
		else if (excludes.Match(relative).HasMatches)
		{
			return RecognizeType.Ignored;
		}
		else
		{
			return RecognizeType.NotCare;
		}
	}
}
