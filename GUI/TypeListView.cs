using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CodingFS.GUI;

// Add ComboBox inside ListView never visible, we have to wrap them with UserControl.
// https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/use-combobox-edit-listview
public sealed partial class TypeListView : UserControl
{
	ListViewItem? lvItem;

	public TypeListView()
	{
		InitializeComponent();
	}

	public void AddPath(string path, FileType type)
	{
		var index = listView.Items.IndexOfKey(path);
		if (index != -1)
		{
			listView.SelectedIndices.Clear();
			listView.SelectedIndices.Add(index);
		}
		else
		{
			var li = new ListViewItem(path);
			listView.Items.Add(li);
			li.Name = path;
			li.SubItems.Add(type.ToString());
		}
	}

	public IEnumerable<(string, FileType)> Items
	{
		get => listView.Items.Cast<ListViewItem>().Select(Item2Entry);
	}

	static (string, FileType) Item2Entry(ListViewItem i)
	{
		return (i.Text, Enum.Parse<FileType>(i.SubItems[1].Text));
	}

	void Combo_SelectedValueChanged(object? sender, EventArgs e)
	{
		combo.Visible = false;
		lvItem!.SubItems[1].Text = combo.Text;
	}

	void ListView_MouseUp(object sender, MouseEventArgs e)
	{
		var cellX = listView.Columns[0].Width;
		var cellWidth = listView.Columns[1].Width;

		lvItem = listView.GetItemAt(e.X, e.Y);
		if (lvItem == null)
		{
			return; // Does not check on any item.
		}
		if (e.X < cellX)
		{
			return; // Click the first column.
		}

		var bounds = lvItem.Bounds;
		if ((bounds.Left + cellWidth) < 0)
		{
			return; // The cell is out of view to the left, do nothing.
		}
		else if (bounds.Left < 0)
		{
			// Determine if column extends beyond right side of ListView.
			if ((bounds.Left + cellWidth) > Width)
			{
				// Set width of column to match width of ListView.
				bounds.X = 0;
				bounds.Width = Width;
			}
			else
			{
				// Right side of cell is in view.
				bounds.X = 2;
				bounds.Width = cellWidth + bounds.Left;
			}
		}
		else if (cellWidth > Width)
		{
			bounds.Width = Width;
		}
		else
		{
			bounds.X = 2;
			bounds.Width = cellWidth;
		}

		// Adjust the top to account for the location of the ListView.
		bounds.Y += listView.Top;
		bounds.X += listView.Left + cellX;

		// Assign calculated bounds to the ComboBox.
		combo.Bounds = bounds;
		combo.Text = lvItem.SubItems[1].Text;

		// Display the ComboBox, and make sure that it is on top with focus.
		combo.Visible = true;
		combo.BringToFront();
		combo.Focus();
	}

	void ListView_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode != Keys.Delete)
		{
			return;
		}
		for (int i = listView.SelectedIndices.Count - 1; i >= 0; i--)
		{
			listView.Items.RemoveAt(listView.SelectedIndices[i]);
		}
	}

	void AddFilesButton_Click(object sender, EventArgs e)
	{
		using var dialog = new OpenFileDialog()
		{
			//InitialDirectory = Path.GetDirectoryName(configFileBox.Text),
		};
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			AddPath(dialog.FileName, FileType.Dependency);
		}
	}

	void AddFolderButton_Click(object sender, EventArgs e)
	{
		using var dialog = new FolderBrowserDialog();
		//dialog.InitialDirectory = rootBox.Text;

		if (dialog.ShowDialog() == DialogResult.OK)
		{
			AddPath(dialog.SelectedPath, FileType.Dependency);
		}
	}
}
