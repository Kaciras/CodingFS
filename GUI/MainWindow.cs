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

		foreach (var item in VirtualFS.GetFreeDrives())
		{
			driveSelect.Items.Add(item);
		}

		SetControlsFromConfiguration();
	}

	void SetControlsFromConfiguration()
	{
		readonlyCheck.Checked = configuration.Mount.Readonly;
		driveSelect.SelectedItem = configuration.Mount.Point;
		rootBox.Text = configuration.Root;
		depthInput.Value = configuration.MaxDepth;

		var type = configuration.Mount.Type;
		sourceCheck.Checked = (type & FileType.Source) != 0;
		dependencyCheck.Checked = (type & FileType.Dependency) != 0;
		generatedCheck.Checked = (type & FileType.Generated) != 0;
	}

	void SetConfigurationFromControls()
	{
		configuration.Mount.Readonly = readonlyCheck.Checked;
		configuration.Root = rootBox.Text;
		configuration.MaxDepth = (int)depthInput.Value;

		if (driveSelect.SelectedItem != null)
		{
			configuration.Mount.Point = (string)driveSelect.SelectedItem;
		}

		configuration.Mount.Type = FileType.None;
		if (sourceCheck.Checked)
		{
			configuration.Mount.Type |= FileType.Source;
		}
		if (dependencyCheck.Checked)
		{
			configuration.Mount.Type |= FileType.Dependency;
		}
		if (generatedCheck.Checked)
		{
			configuration.Mount.Type |= FileType.Generated;
		}
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
		SetConfigurationFromControls();
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
