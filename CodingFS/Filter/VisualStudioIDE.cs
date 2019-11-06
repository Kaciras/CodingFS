using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodingFS.Filter
{
	public class VisualStudioIDE : ClassifierFactory
	{
		// VisualStudio 的 sln 文件里记录了项目的位置，示例：
		// Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "CodingFS", "CodingFS\CodingFS.csproj", "{207E8E66-808C-4026-91D8-62F479792563}"
		// 可以根据前面的“Project(”来识别这一行。
		private readonly Regex ProjectRE = new Regex(@"Project\((.+)", RegexOptions.Multiline);

		public Classifier? Match(string path)
		{
			var ignored = new RecognizedFileMap(path);

			var sln = Directory.EnumerateFiles(path).FirstOrDefault(p => p.EndsWith(".sln"));
			if (sln == null)
			{
				return null;
			}

			var match = ProjectRE.Match(File.ReadAllText(sln));
			while (match.Success)
			{
				var project = match.Groups[1].Value.Split(", ")[1][1..^1];
				var folder = Path.GetDirectoryName(project);

				if (project.EndsWith(".csproj"))
				{
					ignored.Add(Path.Join(folder, "obj"), RecognizeType.Ignored);
					ignored.Add(Path.Join(folder, "bin"), RecognizeType.Ignored);

					// 应该不会有正常文件叫TestResults吧
					ignored.Add("TestResults", RecognizeType.Ignored);
				}
				else if (project.EndsWith(".vcxproj"))
				{
					// C艹的项目会直接生成在解决方案目录里，Trie树会忽略重复的添加
					ignored.Add("Debug", RecognizeType.Ignored);
					ignored.Add("Release", RecognizeType.Ignored);
					ignored.Add(Path.Join(folder, "Debug"), RecognizeType.Ignored);
					ignored.Add(Path.Join(folder, "Release"), RecognizeType.Ignored);
				}
				match = match.NextMatch();
			}

			return new VisualStudioClassifier(path, ignored);
		}
	}

	public class VisualStudioClassifier : Classifier
	{
		private readonly string folder;
		private readonly RecognizedFileMap ignored;

		public VisualStudioClassifier(string folder, RecognizedFileMap ignored)
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
}
