using System.Diagnostics;

namespace CodingFS.GUI;

public partial class MainWindow : Form
{
	VirtualFS virtualFS = null!;

	public MainWindow()
	{
		InitializeComponent();

		typeSelect.Items.Add(FileType.Source);
		typeSelect.Items.Add(FileType.Dependency);
		typeSelect.Items.Add(FileType.Generated);

		var usedDrives = DriveInfo.GetDrives();
		var freeDrives = Enumerable.Range('A', 'Z' - 'A' + 1)
			.Select(i => (char)i + ":\\")
			.Except(usedDrives.Select(s => s.Name));

		foreach (var item in freeDrives)
		{
			driveSelect.Items.Add(item);
		}
		driveSelect.SelectedIndex = 25 - usedDrives.Length;
	}

	void selectRootButton_Click(object sender, EventArgs e)
	{
		using var dialog = new FolderBrowserDialog();
		dialog.InitialDirectory = rootBox.Text;

		if (dialog.ShowDialog() == DialogResult.OK)
		{
			rootBox.Text = dialog.SelectedPath;
		}
	}

	void mountButton_Click(object sender, EventArgs e)
	{
		var config = new Cli.Config();
		var filter = new MappedPathFilter();
		var scanner = config.CreateScanner();
		var top = Path.GetFileName(scanner.Root);

		var Type = (FileType)typeSelect.SelectedItem;
		if ((Type & FileType.Source) == 0)
		{
			Console.Write($"Type does not contain Source, requires pre-scan files...");
			var watch = new Stopwatch();
			watch.Start();
			filter.Set(top, new PrebuiltPathFilter(scanner, Type));
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{watch.ElapsedMilliseconds}ms.\n");
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		else
		{
			filter.Set(top, new CodingPathFilter(scanner, Type));
		}

		virtualFS = new VirtualFS(filter, new()
		{
#if DEBUG
			Debug = true,
#else
			Debug = false,
#endif
			Name = "CodingFS",
			Readonly = readonlyCheck.Checked,
			MountPoint = "x:",
		});

		mountButton.Enabled = false;
		unmountButton.Enabled = true;
	}

	void unmountButton_Click(object sender, EventArgs e)
	{
		virtualFS.Dispose();
		mountButton.Enabled = true;
		unmountButton.Enabled = false;
	}

	void listButton_Click(object sender, EventArgs e)
	{

	}
}
