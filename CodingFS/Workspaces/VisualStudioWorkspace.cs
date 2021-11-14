using System.IO;
using System.Linq;
using Microsoft.Build.Construction;

namespace CodingFS.Workspaces;

public class VisualStudioIDE : IWorkspaceFactory
{
	public IWorkspace? Match(string path)
	{
		var ignored = new PathDict(path);

		var sln = Directory.EnumerateFiles(path).FirstOrDefault(p => p.EndsWith(".sln"));
		if (sln == null)
		{
			return null;
		}

		var solution = SolutionFile.Parse(sln);
		foreach (var project in solution.ProjectsInOrder)
		{
			var type = Path.GetExtension(project.RelativePath);
			var folder = Path.GetDirectoryName(project.AbsolutePath);

			if (type == ".csproj")
			{
				ignored.Add(Path.Join(folder, "obj"), RecognizeType.Ignored);
				ignored.Add(Path.Join(folder, "bin"), RecognizeType.Ignored);

				// 应该不会有正常文件叫TestResults吧
				ignored.Add("TestResults", RecognizeType.Ignored);
			}
			else if (type == ".vcxproj")
			{
				// C艹的项目会直接生成在解决方案目录里，Trie树会忽略重复的添加
				ignored.Add("Debug", RecognizeType.Ignored);
				ignored.Add("Release", RecognizeType.Ignored);
				ignored.Add(Path.Join(folder, "Debug"), RecognizeType.Ignored);
				ignored.Add(Path.Join(folder, "Release"), RecognizeType.Ignored);
			}
		}

		return new VisualStudioWorkspace(path, ignored);
	}
}

public class VisualStudioWorkspace : IWorkspace
{
	private readonly string folder;
	private readonly PathDict ignored;

	public VisualStudioWorkspace(string folder, PathDict ignored)
	{
		this.folder = folder;
		this.ignored = ignored;
	}

	public RecognizeType Recognize(string file)
	{
		var rpath = Path.GetRelativePath(folder, file);
		if (Directory.Exists(file))
		{
			if (rpath == "packages" || rpath == ".vs")
			{
				return RecognizeType.Dependency;
			}
		}
		return ignored.Recognize(rpath);
	}
}
