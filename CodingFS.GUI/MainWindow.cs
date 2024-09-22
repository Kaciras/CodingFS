using System.Diagnostics;

namespace CodingFS.GUI;

public sealed partial class MainWindow : Form
{
	VirtualFS virtualFS = null!;

	public MainWindow()
	{
		InitializeComponent();

		typeSelect.Items.Add(FileType.Source);
		typeSelect.Items.Add(FileType.Dependency);
		typeSelect.Items.Add(FileType.Generated);

		foreach (var item in VirtualFS.GetFreeDrives())
		{
			driveSelect.Items.Add(item);
		}
		driveSelect.SelectedIndex = driveSelect.Items.Count - 1;
	}

	void SelectRootButton_Click(object sender, EventArgs e)
	{
		using var dialog = new FolderBrowserDialog();
		dialog.InitialDirectory = rootBox.Text;

		if (dialog.ShowDialog() == DialogResult.OK)
		{
			rootBox.Text = dialog.SelectedPath;
		}
	}

	void MountButton_Click(object sender, EventArgs e)
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

	void UnmountButton_Click(object sender, EventArgs e)
	{
		virtualFS.Dispose();
		mountButton.Enabled = true;
		unmountButton.Enabled = false;
	}

	void ListButton_Click(object sender, EventArgs e)
	{

	}
}
