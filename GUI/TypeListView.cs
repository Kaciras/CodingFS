using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodingFS.Cli;

namespace CodingFS.GUI;

// https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/use-combobox-edit-listview
public partial class TypeListView : UserControl
{
	ListViewItem? lvItem;

	public TypeListView()
	{
		InitializeComponent();
	}

	void Combo_SelectedValueChanged(object? sender, EventArgs e)
	{
		lvItem!.SubItems[1].Text = combo.Text;
		combo.Visible = false;
	}

	public void AddPath(string path, FileType type)
	{
		var li = new ListViewItem(path);
		listView.Items.Add(li);
		li.SubItems.Add(type.ToString());
	}

	public IEnumerable<(string, FileType)> Items
	{
		get => listView.Items.Cast<ListViewItem>().Select(Item2Entry);
	}

	static (string, FileType) Item2Entry(ListViewItem i)
	{
		return (i.Text, Enum.Parse<FileType>(i.SubItems[1].Text));
	}

	void ListView_MouseUp(object sender, MouseEventArgs e)
	{
		// Get the item on the row that is clicked.
		lvItem = listView.GetItemAt(e.X, e.Y);

		// Make sure that an item is clicked.
		if (lvItem == null)
		{
			return;
		}

		// Get the bounds of the item that is clicked.
		Rectangle ClickedItem = lvItem.Bounds;

		// Verify that the column is completely scrolled off to the left.
		if ((ClickedItem.Left + listView.Columns[1].Width) < 0)
		{
			// If the cell is out of view to the left, do nothing.
			return;
		}

		// Verify that the column is partially scrolled off to the left.
		else if (ClickedItem.Left < 0)
		{
			// Determine if column extends beyond right side of ListView.
			if ((ClickedItem.Left + listView.Columns[1].Width) > Width)
			{
				// Set width of column to match width of ListView.
				ClickedItem.Width = Width;
				ClickedItem.X = 0;
			}
			else
			{
				// Right side of cell is in view.
				ClickedItem.Width = listView.Columns[1].Width + ClickedItem.Left;
				ClickedItem.X = 2;
			}
		}
		else if (listView.Columns[1].Width > Width)
		{
			ClickedItem.Width = Width;
		}
		else
		{
			ClickedItem.Width = listView.Columns[1].Width;
			ClickedItem.X = 2;
		}

		// Adjust the top to account for the location of the ListView.
		ClickedItem.Y += listView.Top;
		ClickedItem.X += listView.Left + listView.Columns[0].Width;

		// Assign calculated bounds to the ComboBox.
		combo.Bounds = ClickedItem;

		// Set default text for ComboBox to match the item that is clicked.
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
