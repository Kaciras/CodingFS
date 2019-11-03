using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodingFS.Filter
{
	public class VisualStudioIDE : ClassifierFactory
	{
		private readonly Regex ProjectRE = new Regex(@"Project\((.+)", RegexOptions.Multiline);

		public Classifier? Match(string path)
		{
			var ignored = new PathTrie<RecognizeType>(RecognizeType.NotCare);
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
				}
				if (project.EndsWith(".vcxproj"))
				{
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
		private string folder;
		private PathTrie<RecognizeType> ignored;

		public VisualStudioClassifier(string folder, PathTrie<RecognizeType> ignored)
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
			return ignored.Get(rpath, RecognizeType.Uncertain);
		}
	}
}
