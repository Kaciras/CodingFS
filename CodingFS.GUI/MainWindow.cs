using CodingFS.Cli;

namespace CodingFS.GUI;

public sealed partial class MainWindow : Form
{
	readonly Config configuration;

	VirtualFS? virtualFS;

	public MainWindow(string? configFile)
	{
		configuration = Config.LoadToml(configFile);

		InitializeComponent();

		typeSelect.Items.Add(FileType.Source);
		typeSelect.Items.Add(FileType.Dependency);
		typeSelect.Items.Add(FileType.Generated);

		foreach (var item in VirtualFS.GetFreeDrives())
		{
			driveSelect.Items.Add(item);
		}

		SetDataFromConfiguration();
	}

	void SetDataFromConfiguration()
	{
		rootBox.Text = configuration.Root;
		readonlyCheck.Checked = configuration.Mount.Readonly;
		typeSelect.SelectedItem = configuration.Mount.Type;
		driveSelect.SelectedItem = configuration.Mount.Point;
	}

	void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
	{
		virtualFS?.Dispose();
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
		virtualFS = MountCommand.CreateVirtualFS(configuration);
		optionsGroup.Enabled = false;
		mountButton.Enabled = false;
		unmountButton.Enabled = true;
	}

	void UnmountButton_Click(object sender, EventArgs e)
	{
		virtualFS!.Dispose();
		optionsGroup.Enabled = true;
		mountButton.Enabled = true;
		unmountButton.Enabled = false;
	}
}
