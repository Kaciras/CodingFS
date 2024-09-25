using CodingFS.Cli;

namespace CodingFS.GUI;

public sealed partial class MainWindow : Form
{
	Config configuration;
	VirtualFS? virtualFS;

	public MainWindow(string? configFile)
	{
		configFile ??= Config.DEFAULT_CONFIG_FILE;
		configFile = Path.GetFullPath(configFile);

		InitializeComponent();

		foreach (var item in VirtualFS.GetFreeDrives())
		{
			driveSelect.Items.Add(item);
		}
		configFileBox.Text = configFile;

		configuration = Config.LoadToml(configFile);
		SetControlsFromConfiguration();
	}

	void SetControlsFromConfiguration()
	{
		readonlyCheck.Checked = configuration.Mount.Readonly;
		driveSelect.SelectedItem = configuration.Mount.Point;
		rootBox.Text = configuration.Root;
		depthInput.Value = configuration.MaxDepth;

		foreach (var item in configuration.Deps)
		{
			listView.AddPath(item, FileType.Dependency);
		}
		foreach (var item in configuration.Generated)
		{
			listView.AddPath(item, FileType.Generated);
		}

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
		configuration.Deps.Clear();
		configuration.Generated.Clear();

		foreach (var (path, type) in listView.Items)
		{
			(type == FileType.Generated ? configuration.Generated : configuration.Deps).Add(path);
		}

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

	void SelectConfigButton_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog()
		{
			Filter = "Toml (*.toml)|*.toml",
			InitialDirectory = Path.GetDirectoryName(configFileBox.Text),
		};
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			configFileBox.Text = dialog.FileName;
			configuration = Config.LoadToml(dialog.FileName);
			SetControlsFromConfiguration();
		}
	}

	void SaveConfigButton_Click(object sender, EventArgs e)
	{
		using var dialog = new SaveFileDialog()
		{
			Filter = "Toml (*.toml)|*.toml",
			DefaultExt = "toml",
			FileName = Path.GetFileName(configFileBox.Text),
			InitialDirectory = Path.GetDirectoryName(configFileBox.Text),
		};
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			SetConfigurationFromControls();
			configFileBox.Text = dialog.FileName;
			configuration.SaveToml(dialog.FileName);
		}
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

		listView.Enabled = false;
		optionsGroup.Enabled = false;
		mountButton.Enabled = false;
		unmountButton.Enabled = true;
		unmountButton.Focus();
		mountedLabel.Visible = true;
		selectConfigButton.Enabled = false;
	}

	void UnmountButton_Click(object sender, EventArgs e)
	{
		virtualFS!.Dispose();

		listView.Enabled = true;
		optionsGroup.Enabled = true;
		mountButton.Enabled = true;
		unmountButton.Enabled = false;
		mountButton.Focus();
		mountedLabel.Visible = false;
		selectConfigButton.Enabled = true;
	}
}
