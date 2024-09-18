using System.Runtime.CompilerServices;

[module: SkipLocalsInit]

namespace CodingFS.GUI;

static class Program
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new MainWindow());
	}
}
